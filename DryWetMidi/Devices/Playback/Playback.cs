using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class Playback
    {
        #region Constants

        private const uint ClockInterval = 1; // ms

        #endregion

        #region Fields

        private readonly IEnumerable<TimedEvent> _events;
        private readonly IEnumerator<TimedEvent> _eventsEnumerator;

        private readonly TempoMap _tempoMap;
        private readonly OutputDevice _outputDevice;

        private readonly MidiClock _clock;

        #endregion

        #region Constructor

        private Playback(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            _events = events.Select(e => new TrackChunk(e.OfType<ChannelEvent>())).GetTimedEvents();
            _eventsEnumerator = _events.GetEnumerator();
            _eventsEnumerator.MoveNext();

            _tempoMap = tempoMap;
            _outputDevice = outputDevice;

            _clock = new MidiClock(ClockInterval, tempoMap);
            _clock.Tick += OnClockTick;
        }

        #endregion

        #region Methods

        public void Start()
        {
            _clock.Start();
        }

        public void Stop()
        {
            _clock.Stop();
        }

        public void Pause()
        {
            _clock.Pause();
        }

        public static Playback Create(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return new Playback(events, tempoMap, outputDevice);
        }

        private void OnClockTick(object sender, TickEventArgs e)
        {
            var ticks = e.Ticks;

            do
            {
                var timedEvent = _eventsEnumerator.Current;
                if (timedEvent == null)
                    continue;

                if (timedEvent.Time > ticks)
                    return;

                _outputDevice.SendEvent(timedEvent.Event);
            }
            while (_eventsEnumerator.MoveNext());

            Stop();
        }

        #endregion
    }
}
