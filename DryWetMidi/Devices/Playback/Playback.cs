using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class Playback : IDisposable
    {
        #region Constants

        private const uint ClockInterval = 1; // ms

        #endregion

        #region Fields

        private readonly IEnumerator<TimedEvent> _eventsEnumerator;

        private readonly TempoMap _tempoMap;
        private readonly OutputDevice _outputDevice;

        private readonly MidiClock _clock;
        private readonly HashSet<NoteId> _noteIds = new HashSet<NoteId>();

        private bool _disposed = false;

        #endregion

        #region Constructor

        private Playback(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            var timedEvents = events.Select(e => new TrackChunk(e.OfType<ChannelEvent>())).GetTimedEvents();
            _eventsEnumerator = timedEvents.GetEnumerator();
            _eventsEnumerator.MoveNext();

            _tempoMap = tempoMap;
            _outputDevice = outputDevice;

            _clock = new MidiClock(ClockInterval, tempoMap);
            _clock.Tick += OnClockTick;
        }

        #endregion

        #region Finalizer

        ~Playback()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public bool Repeat { get; set; }

        #endregion

        #region Methods

        public void Start()
        {
            EnsureIsNotDisposed();

            WarmUpDevice();

            _clock.Start();
        }

        public void Stop()
        {
            EnsureIsNotDisposed();

            _clock.Stop();
            StopCurrentNotes();
        }

        public void Pause()
        {
            EnsureIsNotDisposed();

            _clock.Pause();
            StopCurrentNotes();
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
            if (_clock.State != MidiClockState.Running)
                return;

            var ticks = e.Ticks;

            do
            {
                var timedEvent = _eventsEnumerator.Current;
                if (timedEvent == null)
                    continue;

                if (timedEvent.Time > ticks)
                    return;

                var midiEvent = timedEvent.Event;

                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent != null)
                    _noteIds.Add(noteOnEvent.GetNoteId());

                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                    _noteIds.Remove(noteOffEvent.GetNoteId());

                _outputDevice.SendEvent(midiEvent);
            }
            while (_eventsEnumerator.MoveNext());

            if (!Repeat)
                Stop();

            _clock.Restart();
            _eventsEnumerator.Reset();
            _eventsEnumerator.MoveNext();
        }

        private void EnsureIsNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Playback is disposed.");
        }

        private void WarmUpDevice()
        {
            _outputDevice.SendEvent(new NoteOnEvent((SevenBitNumber)0, (SevenBitNumber)0));
            _outputDevice.SendEvent(new NoteOffEvent((SevenBitNumber)0, (SevenBitNumber)0));
        }

        private void StopCurrentNotes()
        {
            foreach (var noteId in _noteIds)
            {
                _outputDevice.SendEvent(new NoteOffEvent(noteId.NoteNumber, (SevenBitNumber)0) { Channel = noteId.Channel });
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();

                _clock.Tick -= OnClockTick;
                _clock.Dispose();
                _eventsEnumerator.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
