using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    internal sealed class MidiClock : IDisposable
    {
        #region Events

        public event EventHandler<TickEventArgs> Tick;

        #endregion

        #region Fields

        private bool _disposed = false;

        private readonly uint _interval;
        private readonly TempoMap _tempoMap;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private uint _resolution;
        private MidiTimerWinApi.TimeProc _tickCallback;
        private uint _timerId;

        #endregion

        #region Constructor

        public MidiClock(uint interval, TempoMap tempoMap)
        {
            _interval = interval;
            _tempoMap = tempoMap;
        }

        #endregion

        #region Finalizer

        ~MidiClock()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public MidiClockState State { get; private set; }

        #endregion

        #region Methods

        public void Start()
        {
            switch (State)
            {
                case MidiClockState.Running:
                    return;

                case MidiClockState.Stopped:
                    {
                        var timeCaps = default(MidiTimerWinApi.TIMECAPS);
                        MidiTimerWinApi.timeGetDevCaps(ref timeCaps, (uint)Marshal.SizeOf(timeCaps));

                        _resolution = Math.Min(Math.Max(timeCaps.wPeriodMin, _interval), timeCaps.wPeriodMax);
                        _tickCallback = OnTick;

                        _stopwatch.Restart();
                    }
                    break;

                case MidiClockState.Paused:
                    _stopwatch.Start();
                    break;
            }

            // TODO: process errors

            MidiTimerWinApi.timeBeginPeriod(_resolution);
            _timerId = MidiTimerWinApi.timeSetEvent(_interval, _resolution, _tickCallback, IntPtr.Zero, MidiTimerWinApi.TIME_PERIODIC);

            State = MidiClockState.Running;
        }

        public void Stop()
        {
            State = MidiClockState.Stopped;
            StopTimer();
        }

        public void Pause()
        {
            State = MidiClockState.Paused;
            StopTimer();
        }

        private void OnTick(uint uID, uint uMsg, uint dwUser, uint dw1, uint dw2)
        {
            var elapsed = _stopwatch.Elapsed;
            var ticks = TimeConverter.ConvertFrom((MetricTimeSpan)elapsed, _tempoMap);

            Tick?.Invoke(this, new TickEventArgs(ticks));
        }

        private void StopTimer()
        {
            // TODO: process errors

            _stopwatch.Stop();
            MidiTimerWinApi.timeEndPeriod(_resolution);
            MidiTimerWinApi.timeKillEvent(_timerId);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: prevent exceptions
            Stop();

            _disposed = true;
        }

        #endregion
    }
}
