using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class OutputDevice : MidiDevice
    {
        #region Constructor

        internal OutputDevice(uint id)
            : base(id)
        {
            SetDeviceInformation();
        }

        #endregion

        #region Properties

        public OutputDeviceType DeviceType { get; private set; }

        public int VoicesNumber { get; private set; }

        public int NotesNumber { get; private set; }

        public IEnumerable<FourBitNumber> Channels { get; private set; }

        #endregion

        #region Methods

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
        }

        private void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

        private static void ProcessMmResult(Func<MMRESULT> method)
        {
            var mmResult = method();
            if (mmResult == MMRESULT.MMSYSERR_NOERROR)
                return;

            var stringBuilder = new StringBuilder((int)MidiOutWinApi.MAXERRORLENGTH);
            var getErrorTextResult = MidiOutWinApi.midiOutGetErrorText(mmResult, stringBuilder, MidiOutWinApi.MAXERRORLENGTH + 1);
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
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        #endregion
    }
}
