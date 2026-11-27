using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.Audio
{
    /// <summary>
    /// A bell-like arpeggio, one note after another, each ringing out. A rising major chord marks a claim or a win;
    /// a falling minor one marks defeat.
    /// </summary>
    public sealed class ChimeRecipe : ISoundRecipe
    {
        private const float RingTimeConstant = 0.4f;
        private const float RingOutSeconds = 1.2f;
        private const float SecondHarmonicLevel = 0.35f;
        private const float ThirdHarmonicLevel = 0.12f;
        private const float AttackSeconds = 0.005f;
        private const float Peak = 0.7f;

        private readonly IReadOnlyList<float> _notesHertz;
        private readonly float _noteGapSeconds;

        public ChimeRecipe(IReadOnlyList<float> notesHertz, float noteGapSeconds)
        {
            if (notesHertz == null || notesHertz.Count == 0)
            {
                throw new ArgumentException("A chime needs at least one note.", nameof(notesHertz));
            }

            if (notesHertz.Any(note => !(note > 0f) || float.IsInfinity(note)))
            {
                throw new ArgumentOutOfRangeException(nameof(notesHertz), "Every note must be a positive frequency.");
            }

            if (!(noteGapSeconds >= 0f) || float.IsInfinity(noteGapSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(noteGapSeconds), noteGapSeconds, "The gap must be a non-negative number.");
            }

            _notesHertz = notesHertz.ToList();
            _noteGapSeconds = noteGapSeconds;
        }

        public SoundChannel Channel => SoundChannel.Interface;

        public float[] Render(int sampleRate)
        {
            float duration = (_noteGapSeconds * (_notesHertz.Count - 1)) + RingOutSeconds;
            var samples = new float[Signal.SampleCount(duration, sampleRate)];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float sum = 0f;
                for (int note = 0; note < _notesHertz.Count; note++)
                {
                    sum += Bell(_notesHertz[note], time - (note * _noteGapSeconds));
                }

                samples[index] = sum;
            }

            return Signal.Normalise(samples, Peak);
        }

        private static float Bell(float frequencyHertz, float sinceStrike)
        {
            if (sinceStrike < 0f)
            {
                return 0f;
            }

            float tone = Signal.Sine(frequencyHertz, sinceStrike)
                + (Signal.Sine(frequencyHertz * 2f, sinceStrike) * SecondHarmonicLevel)
                + (Signal.Sine(frequencyHertz * 3f, sinceStrike) * ThirdHarmonicLevel);
            float attack = Math.Min(1f, sinceStrike / AttackSeconds);
            return tone * attack * Signal.Decay(sinceStrike, RingTimeConstant);
        }
    }
}
