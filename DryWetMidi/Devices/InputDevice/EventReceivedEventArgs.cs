using System;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class EventReceivedEventArgs
    {
        #region Constructor

        internal EventReceivedEventArgs(MidiEvent midiEvent, DateTime time)
        {
            Event = midiEvent;
            Time = time;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public DateTime Time { get; }

        #endregion
    }
}
