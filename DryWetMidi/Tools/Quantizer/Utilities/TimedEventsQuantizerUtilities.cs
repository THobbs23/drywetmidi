﻿using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to quantize timed events time.
    /// </summary>
    public static class TimedEventsQuantizerUtilities
    {
        #region Methods

        /// <summary>
        /// Quantizes timed events contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to quantize timed events in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which timed events should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void QuantizeTimedEvents(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var timedEventsManager = trackChunk.ManageTimedEvents())
            {
                new TimedEventsQuantizer().Quantize(timedEventsManager.Events, grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes timed events contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to quantize timed events in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which timed events should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void QuantizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeTimedEvents(grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes timed events contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to quantize timed events in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="settings">Settings according to which timed events should be quantized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="grid"/> is null.</exception>
        public static void QuantizeTimedEvents(this MidiFile midiFile, IGrid grid, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeTimedEvents(grid, tempoMap, settings);
        }

        #endregion
    }
}
