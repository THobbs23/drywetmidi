using System;
using System.IO;
using System.Text;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public abstract class MidiDevice : IDisposable
    {
        #region Constants

        private const int ParameterBufferSize = 2;
        private static readonly ReadingSettings ReadingSettings = new ReadingSettings();

        #endregion

        #region Fields

        protected readonly uint _id;
        protected IntPtr _handle = IntPtr.Zero;
        protected bool _disposed = false;
        private readonly MemoryStream _memoryStream = new MemoryStream(ParameterBufferSize);
        private readonly MidiReader _midiReader;
        private MidiWinApi.MidiMessageCallback _callback;

        #endregion

        #region Constructor

        internal MidiDevice(uint id)
        {
            _id = id;
            _midiReader = new MidiReader(_memoryStream);
        }

        #endregion

        #region Finalizer

        ~MidiDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        public Manufacturer DriverManufacturer { get; private set; }

        public ushort ProductIdentifier { get; private set; }

        public Version DriverVersion { get; private set; }

        #endregion

        #region Methods

        protected void SetBasicDeviceInformation(ushort manufacturerIdentifier,
                                          ushort productIdentifier,
                                          uint driverVersion,
                                          string name)
        {
            Name = name;
            DriverManufacturer = Enum.IsDefined(typeof(Manufacturer), manufacturerIdentifier)
                ? (Manufacturer)manufacturerIdentifier
                : Manufacturer.Unknown;
            ProductIdentifier = productIdentifier;

            var majorVersion = driverVersion >> 8;
            var minorVersion = driverVersion & 0xFF;
            DriverVersion = new Version((int)majorVersion, (int)minorVersion);
        }

        protected void EnsureDeviceIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Device is disposed.");
        }

        protected void Open()
        {
            _callback = OnMessage;
            ProcessMmResult(() => OpenDevice(out _handle, _id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction));
        }

        protected static void WriteBytesToStream(MemoryStream memoryStream, params byte[] bytes)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Write(bytes, 0, bytes.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);
        }

        protected abstract void OnEvent(MidiEvent midiEvent, int milliseconds);

        internal void ProcessMmResult(Func<MMRESULT> method)
        {
            var mmResult = method();
            if (mmResult == MMRESULT.MMSYSERR_NOERROR)
                return;

            var stringBuilder = new StringBuilder((int)MidiWinApi.MaxErrorLength);
            var getErrorTextResult = GetErrorText(mmResult, stringBuilder, MidiWinApi.MaxErrorLength + 1);
            if (getErrorTextResult != MMRESULT.MMSYSERR_NOERROR)
                throw new MidiDeviceException("Error occured but failed to get description for it.");

            var errorText = stringBuilder.ToString();
            throw new MidiDeviceException(errorText);
        }

        internal abstract MMRESULT OpenDevice(out IntPtr lpmidi, uint uDeviceID, MidiWinApi.MidiMessageCallback dwCallback, IntPtr dwInstance, uint dwFlags);

        internal abstract MMRESULT CloseDevice(IntPtr hmidi);

        internal abstract MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText);

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            var parameter1 = dwParam1.ToInt32();
            var parameter2 = dwParam2.ToInt32();

            switch (wMsg)
            {
                case MidiMessage.MIM_DATA:
                    OnMessage(parameter1, parameter2);
                    break;

                case MidiMessage.MIM_ERROR:
                    break;
            }
        }

        private void OnMessage(int message, int milliseconds)
        {
            // TODO: move bit operations to DataTypesUtilities
            WriteBytesToStream(_memoryStream, (byte)((message & 0xFF00) >> 8), (byte)(message >> 16));

            var statusByte = (byte)(message & 0xFF);
            var eventReader = EventReaderFactory.GetReader(statusByte);
            var midiEvent = eventReader.Read(_midiReader, ReadingSettings, statusByte);

            OnEvent(midiEvent, milliseconds);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_handle == IntPtr.Zero)
                    return;

                CloseDevice(_handle);

                _memoryStream.Dispose();
                _midiReader.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
