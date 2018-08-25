using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class InputDevice : MidiDevice
    {
        #region Fields

        private DateTime _startTime;

        #endregion

        #region Events

        public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

        #endregion

        #region Constructor

        internal InputDevice(uint id)
            : base(id)
        {
            SetDeviceInformation();
        }

        #endregion

        #region Methods

        public void Start()
        {
            EnsureDeviceIsNotDisposed();

            Open();
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

        protected override void OnEvent(MidiEvent midiEvent, int milliseconds)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent, _startTime.AddMilliseconds(milliseconds)));
        }

        internal override MMRESULT OpenDevice(out IntPtr lpmidi, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags)
        {
            return MidiInWinApi.midiInOpen(out lpmidi, uDeviceID, dwCallback, dwInstance, dwFlags);
        }

        internal override MMRESULT CloseDevice(IntPtr hmidi)
        {
            return MidiInWinApi.midiInClose(hmidi);
        }

        internal override MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiInWinApi.midiInGetErrorText(mmrError, pszText, cchText);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiInWinApi.MIDIINCAPS);
            ProcessMmResult(() => MidiInWinApi.midiInGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);
        }

        #endregion
    }
}
