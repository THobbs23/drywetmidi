using System;
using System.Collections.Generic;
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

        private readonly IEnumerator<PlaybackEvent> _eventsEnumerator;

        private readonly TempoMap _tempoMap;
        private readonly OutputDevice _outputDevice;

        private readonly MidiClock _clock;
        private readonly HashSet<NoteId> _noteIds = new HashSet<NoteId>();

        private bool _disposed = false;

        #endregion

        #region Constructor

        private Playback(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap, OutputDevice outputDevice)
        {
            _eventsEnumerator = GetPlaybackEvents(events, tempoMap).GetEnumerator();
            _eventsEnumerator.MoveNext();

            _tempoMap = tempoMap;
            _outputDevice = outputDevice;

            _clock = new MidiClock(ClockInterval);
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

        public bool Loop { get; set; }

        public bool InterruptNotesOnStop { get; set; } = true;

        #endregion

        #region Methods

        public void Start()
        {
            EnsureIsNotDisposed();

            if (_clock.State == MidiClockState.Running)
                return;

            WarmUpDevice();

            _clock.Start();
        }

        public void Stop()
        {
            EnsureIsNotDisposed();

            if (_clock.State == MidiClockState.Stopped)
                return;

            _clock.Stop();
            InterruptNotes();
        }

        public void Pause()
        {
            EnsureIsNotDisposed();

            if (_clock.State == MidiClockState.Paused)
                return;

            _clock.Pause();
            InterruptNotes();
        }

        public void MoveToTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            Stop();

            _clock.Time = TimeConverter.ConvertTo<MetricTimeSpan>(time, _tempoMap);

            _eventsEnumerator.Reset();
            do
            {
                _eventsEnumerator.MoveNext();
            }
            while (_eventsEnumerator.Current.Time < _clock.Time);

            Start();
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
            var time = e.Time;

            do
            {
                var timedEvent = _eventsEnumerator.Current;
                if (timedEvent == null)
                    continue;

                if (timedEvent.Time > time)
                    return;

                var midiEvent = timedEvent.Event;

                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent != null)
                    _noteIds.Add(noteOnEvent.GetNoteId());

                var noteOffEvent = midiEvent as NoteOffEvent;
                if (noteOffEvent != null)
                    _noteIds.Remove(noteOffEvent.GetNoteId());

                if (_clock.State != MidiClockState.Running)
                    return;

                _outputDevice.SendEvent(midiEvent);
            }
            while (_eventsEnumerator.MoveNext());

            if (!Loop)
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

        private void InterruptNotes()
        {
            if (!InterruptNotesOnStop)
                return;

            foreach (var noteId in _noteIds)
            {
                _outputDevice.SendEvent(new NoteOffEvent(noteId.NoteNumber, SevenBitNumber.MinValue) { Channel = noteId.Channel });
            }
        }

        private static IEnumerable<PlaybackEvent> GetPlaybackEvents(IEnumerable<IEnumerable<MidiEvent>> events, TempoMap tempoMap)
        {
            return events.Select(e => new TrackChunk(e.Where(midiEvent => !(midiEvent is MetaEvent))))
                         .GetTimedEvents()
                         .Select(e => new PlaybackEvent(e.Event, e.TimeAs<MetricTimeSpan>(tempoMap)))
                         .ToList();
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
