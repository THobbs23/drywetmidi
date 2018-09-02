using System;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class TickEventArgs : EventArgs
    {
        #region Constructor

        public TickEventArgs(long ticks)
        {
            Ticks = ticks;
        }

        #endregion

        #region Properties

        public long Ticks { get; }

        #endregion
    }
}
