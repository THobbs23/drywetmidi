using System;

namespace Melanchall.DryWetMidi.Devices
{
    public abstract class MidiDevice : IDisposable
    {
        #region Fields

        protected readonly uint _id;
        protected IntPtr _handle = IntPtr.Zero;
        protected bool _disposed = false;

        #endregion

        #region Constructor

        protected MidiDevice(uint id)
        {
            _id = id;
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

        public Manufacturer Manufacturer { get; private set; }

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
            Manufacturer = Enum.IsDefined(typeof(Manufacturer), manufacturerIdentifier)
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

        protected abstract void Dispose(bool disposing);

        #endregion
    }
}
