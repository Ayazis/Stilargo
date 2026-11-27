using System;

namespace Stilargo.Core.Audio
{
    /// <summary>A bullet striking home: a short, dull smack with a little grit on top.</summary>
    public sealed class ImpactRecipe : ISoundRecipe
    {
        private const float DurationSeconds = 0.18f;
        private const float GritCutoffHertz = 2400f;
        private const float GritTimeConstant = 0.015f;
        private const float SmackHertz = 170f;
        private const float SmackTimeConstant = 0.03f;
        private const float Peak = 0.8f;

        private readonly int _seed;

        public ImpactRecipe(int seed)
        {
            _seed = seed;
        }

        public SoundChannel Channel => SoundChannel.Effects;

        public float[] Render(int sampleRate)
        {
            var random = new Random(_seed);
            var samples = new float[Signal.SampleCount(DurationSeconds, sampleRate)];
            float gritFactor = Signal.LowPassFactor(GritCutoffHertz, sampleRate);
            float grit = 0f;
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                grit += gritFactor * (Signal.Noise(random) - grit);
                float smack = Signal.Sine(SmackHertz, time) * Signal.Decay(time, SmackTimeConstant);
                samples[index] = (grit * Signal.Decay(time, GritTimeConstant)) + smack;
            }

            return Signal.Normalise(samples, Peak);
        }
    }
}
