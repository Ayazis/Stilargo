using System;

namespace Stilargo.Core.Audio
{
    /// <summary>Three hammer knocks on wood, for a fortification going up.</summary>
    public sealed class HammerRecipe : ISoundRecipe
    {
        private const int KnockCount = 3;
        private const float KnockGapSeconds = 0.17f;
        private const float DurationSeconds = 0.6f;
        private const float WoodCutoffHertz = 900f;
        private const float WoodTimeConstant = 0.025f;
        private const float KnockHertz = 230f;
        private const float KnockTimeConstant = 0.04f;
        private const float Peak = 0.75f;

        private readonly int _seed;

        public HammerRecipe(int seed)
        {
            _seed = seed;
        }

        public SoundChannel Channel => SoundChannel.Effects;

        public float[] Render(int sampleRate)
        {
            var random = new Random(_seed);
            var samples = new float[Signal.SampleCount(DurationSeconds, sampleRate)];
            float woodFactor = Signal.LowPassFactor(WoodCutoffHertz, sampleRate);
            float wood = 0f;
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                wood += woodFactor * (Signal.Noise(random) - wood);
                float sum = 0f;
                for (int knock = 0; knock < KnockCount; knock++)
                {
                    float since = time - (knock * KnockGapSeconds);
                    if (since >= 0f)
                    {
                        sum += (wood * Signal.Decay(since, WoodTimeConstant)) + (Signal.Sine(KnockHertz, since) * Signal.Decay(since, KnockTimeConstant));
                    }
                }

                samples[index] = sum;
            }

            return Signal.Normalise(samples, Peak);
        }
    }
}
