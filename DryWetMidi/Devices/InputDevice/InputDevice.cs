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
    public sealed class InputDevice : MidiDevice
    {
        #region Constants

        private const int ParameterBufferSize = 2;
        private static readonly ReadingSettings ReadingSettings = new ReadingSettings();

        #endregion

        #region Events

        public event EventHandler<EventReceivedEventArgs> EventReceived;

        #endregion

        #region Fields

        private readonly MemoryStream _memoryStream = new MemoryStream(ParameterBufferSize);
        private DateTime _startTime;
        private readonly MidiReader _midiReader;

        #endregion

        #region Constructor

        internal InputDevice(uint id)
            : base(id)
        {
            _midiReader = new MidiReader(_memoryStream);

            SetDeviceInformation();
        }

        #endregion

        #region Methods

        public void Start()
        {
            EnsureDeviceIsNotDisposed();

            ProcessMmResult(() => MidiInWinApi.midiInOpen(out _handle, _id, OnMessage, IntPtr.Zero, CALLBACK.CALLBACK_FUNCTION));
            ProcessMmResult(() => MidiInWinApi.midiInStart(_handle));
            _startTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(() => MidiInWinApi.midiInStop(_handle));
        }

        public void Reset()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(() => MidiInWinApi.midiInReset(_handle));
            _startTime = DateTime.UtcNow;
        }

        public static int GetDevicesCount()
        {
            // TODO: process last error
            // TODO: uint instead of int
            return MidiInWinApi.midiInGetNumDevs();
        }

        public static IEnumerable<InputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            return Enumerable.Range(0, devicesCount).Select(i => new InputDevice((uint)i));
        }

        public static InputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            return GetAll().FirstOrDefault(d => d.Name == name);
        }

        private void OnEventReceived(EventReceivedEventArgs args)
        {
            EventReceived?.Invoke(this, args);
        }

        private void OnMessage(IntPtr hMidiIn, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            var parameter1 = dwParam1.ToInt32();
            var parameter2 = dwParam2.ToInt32();

            var messageTime = _startTime.AddMilliseconds(parameter2);

            switch (wMsg)
            {
                case MidiMessage.MIM_DATA:
                    OnMessage(parameter1 & 0xFF00, messageTime);
                    break;

                case MidiMessage.MIM_ERROR:
                    break;
            }
        }

        private void OnMessage(int message, DateTime messageTime)
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.WriteByte((byte)(message & 0xFF00));
            _memoryStream.WriteByte((byte)(message >> 16));
            _memoryStream.Seek(0, SeekOrigin.Begin);

            var statusByte = (byte)(message & 0xFF);
            var eventReader = EventReaderFactory.GetReader(statusByte);
            var midiEvent = eventReader.Read(_midiReader, ReadingSettings, statusByte);

            OnEventReceived(new EventReceivedEventArgs(midiEvent, messageTime));
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiInWinApi.MIDIINCAPS);
            ProcessMmResult(() => MidiInWinApi.midiInGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);
        }

        private static void ProcessMmResult(Func<MMRESULT> method)
        {
            var mmResult = method();
            if (mmResult == MMRESULT.MMSYSERR_NOERROR)
                return;

            var stringBuilder = new StringBuilder((int)MidiInWinApi.MAXERRORLENGTH);
            var getErrorTextResult = MidiInWinApi.midiInGetErrorText(mmResult, stringBuilder, MidiInWinApi.MAXERRORLENGTH + 1);
            if (getErrorTextResult != MMRESULT.MMSYSERR_NOERROR)
                throw new MidiDeviceException("Error occured but failed to get description for it.");

            var errorText = stringBuilder.ToString();
            throw new MidiDeviceException(errorText);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_handle == IntPtr.Zero)
                    return;

                MidiInWinApi.midiInClose(_handle);

                _memoryStream.Dispose();
                _midiReader.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
