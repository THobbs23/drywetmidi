using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class OutputDevice : MidiDevice
    {
        #region Constants

        private const int ChannelEventBufferSize = 3;
        private static readonly byte[] ZeroBuffer = new byte[ChannelEventBufferSize];

        #endregion

        #region Events

        public event EventHandler<MidiEventSentEventArgs> EventSent;

        #endregion

        #region Fields

        private readonly MemoryStream _memoryStream = new MemoryStream(ChannelEventBufferSize);
        private readonly MidiWriter _midiWriter;
        private readonly WritingSettings _writingSettings = new WritingSettings();

        private byte? _runningStatus = null;

        #endregion

        #region Constructor

        internal OutputDevice(uint id)
            : base(id)
        {
            _midiWriter = new MidiWriter(_memoryStream);

            SetDeviceInformation();
        }

        #endregion

        #region Properties

        public OutputDeviceType DeviceType { get; private set; }

        public int VoicesNumber { get; private set; }

        public int NotesNumber { get; private set; }

        public IEnumerable<FourBitNumber> Channels { get; private set; }

        public bool SupportsPatchCaching { get; private set; }

        public bool SupportsLeftRightVolumeControl { get; private set; }

        public bool SupportsVolumeControl { get; private set; }

        public bool UseRunningStatus
        {
            get { return _writingSettings.CompressionPolicy.HasFlag(CompressionPolicy.UseRunningStatus); }
            set { _writingSettings.CompressionPolicy &= ~CompressionPolicy.UseRunningStatus; }
        }

        #endregion

        #region Methods

        public void SendEvent(MidiEvent midiEvent)
        {
            EnsureDeviceIsNotDisposed();

            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            if (midiEvent is MetaEvent)
                throw new ArgumentException("Meta events cannot be sent via MIDI device.", nameof(midiEvent));

            if (_handle == IntPtr.Zero)
                Open();

            var channelEvent = midiEvent as ChannelEvent;
            if (channelEvent != null)
            {
                SendChannelEvent(channelEvent);
                return;
            }

            _runningStatus = null;

            var sysExEvent = midiEvent as SysExEvent;
            if (sysExEvent != null)
            {
                // TODO: implement sending SysEx events
            }
        }

        public static int GetDevicesCount()
        {
            // TODO: process last error
            // TODO: uint instead of int
            return MidiOutWinApi.midiOutGetNumDevs();
        }

        public static IEnumerable<OutputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            return Enumerable.Range(0, devicesCount).Select(i => new OutputDevice((uint)i));
        }

        public static OutputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            return GetAll().FirstOrDefault(d => d.Name == name);
        }

        protected override void OnEvent(MidiEvent midiEvent, int milliseconds)
        {
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }

        internal override MMRESULT OpenDevice(out IntPtr lpmidi, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags)
        {
            return MidiOutWinApi.midiOutOpen(out lpmidi, uDeviceID, dwCallback, dwInstance, dwFlags);
        }

        internal override MMRESULT CloseDevice(IntPtr hmidi)
        {
            return MidiOutWinApi.midiOutClose(hmidi);
        }

        internal override MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiOutWinApi.midiOutGetErrorText(mmrError, pszText, cchText);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiOutWinApi.MIDIOUTCAPS);
            ProcessMmResult(() => MidiOutWinApi.midiOutGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);

            DeviceType = (OutputDeviceType)caps.wTechnology;
            VoicesNumber = caps.wVoices;
            NotesNumber = caps.wNotes;
            Channels = (from i in Enumerable.Range(0, FourBitNumber.MaxValue + 1)
                        let channel = (FourBitNumber)i
                        let isChannelSupported = (caps.wChannelMask >> i) & 1
                        where isChannelSupported == 1
                        select channel).ToArray();

            var support = (MidiOutWinApi.MIDICAPS)caps.dwSupport;
            SupportsPatchCaching = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_CACHE);
            SupportsVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_VOLUME);
            SupportsLeftRightVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_LRVOLUME);
        }

        private void SendChannelEvent(ChannelEvent channelEvent)
        {
            var eventWriter = EventWriterFactory.GetWriter(channelEvent);

            var statusByte = eventWriter.GetStatusByte(channelEvent);
            var writeStatusByte = _runningStatus != statusByte || !UseRunningStatus;
            _runningStatus = statusByte;

            WriteBytesToStream(_memoryStream, ZeroBuffer);
            eventWriter.Write(channelEvent, _midiWriter, _writingSettings, writeStatusByte);

            var bytes = _memoryStream.GetBuffer();
            var message = bytes[0] + (bytes[1] << 8) + (bytes[2] << 16);

            ProcessMmResult(() => MidiOutWinApi.midiOutShortMsg(_handle, (uint)message));
        }

        #endregion
    }
}
