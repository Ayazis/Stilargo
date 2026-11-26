using System;

namespace Stilargo.Core.Audio
{
    /// <summary>The few signal building blocks the sound recipes share: noise, filters, envelopes and normalising.</summary>
    public static class Signal
    {
        private const double TwoPi = Math.PI * 2.0;

        public static int SampleCount(float seconds, int sampleRate)
        {
            if (!(seconds > 0f) || float.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Duration must be a positive number.");
            }

            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");
            }

            return (int)Math.Ceiling(seconds * sampleRate);
        }

        /// <summary>A sample of white noise in [-1, 1].</summary>
        public static float Noise(Random random)
        {
            return (float)((random.NextDouble() * 2.0) - 1.0);
        }

        /// <summary>Exponential decay: 1 at time 0, about 0.37 after <paramref name="timeConstantSeconds"/>.</summary>
        public static float Decay(float seconds, float timeConstantSeconds)
        {
            return seconds < 0f ? 0f : (float)Math.Exp(-seconds / timeConstantSeconds);
        }

        public static float Sine(float frequencyHertz, float seconds)
        {
            return (float)Math.Sin(TwoPi * frequencyHertz * seconds);
        }

        /// <summary>The smoothing factor of a one-pole low-pass filter with this cutoff.</summary>
        public static float LowPassFactor(float cutoffHertz, int sampleRate)
        {
            double rc = 1.0 / (TwoPi * cutoffHertz);
            double dt = 1.0 / sampleRate;
            return (float)(dt / (rc + dt));
        }

        /// <summary>Scales the samples so the loudest one reaches <paramref name="peak"/>; silence stays silent.</summary>
        public static float[] Normalise(float[] samples, float peak)
        {
            float loudest = 0f;
            foreach (float sample in samples)
            {
                loudest = Math.Max(loudest, Math.Abs(sample));
            }

            if (loudest <= 0f)
            {
                return samples;
            }

            float gain = peak / loudest;
            for (int index = 0; index < samples.Length; index++)
            {
                samples[index] *= gain;
            }

            return samples;
        }
    }
}
