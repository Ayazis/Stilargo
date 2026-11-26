using System;

namespace Stilargo.Core.Audio
{
    /// <summary>
    /// A rifle report: a sharp noise crack, a low thump, a filtered body and a long, soft street echo.
    /// A lower pitch gives a heavier gun.
    /// </summary>
    public sealed class GunshotRecipe : ISoundRecipe
    {
        private const float DurationSeconds = 0.6f;
        private const float CrackTimeConstant = 0.01f;
        private const float BodyTimeConstant = 0.07f;
        private const float BodyCutoffHertz = 1600f;
        private const float ThumpHertz = 95f;
        private const float ThumpTimeConstant = 0.045f;
        private const float ThumpLevel = 0.8f;
        private const float EchoCutoffHertz = 420f;
        private const float EchoDelaySeconds = 0.06f;
        private const float EchoTimeConstant = 0.22f;
        private const float EchoLevel = 0.35f;
        private const float CrackLevel = 0.6f;
        private const float Peak = 0.9f;

        private readonly float _pitch;
        private readonly int _seed;

        public GunshotRecipe(float pitch, int seed)
        {
            if (!(pitch > 0f) || float.IsInfinity(pitch))
            {
                throw new ArgumentOutOfRangeException(nameof(pitch), pitch, "Pitch must be a positive number.");
            }

            _pitch = pitch;
            _seed = seed;
        }

        public SoundChannel Channel => SoundChannel.Effects;

        public float[] Render(int sampleRate)
        {
            var random = new Random(_seed);
            var samples = new float[Signal.SampleCount(DurationSeconds, sampleRate)];
            float bodyFactor = Signal.LowPassFactor(BodyCutoffHertz * _pitch, sampleRate);
            float echoFactor = Signal.LowPassFactor(EchoCutoffHertz * _pitch, sampleRate);
            float body = 0f;
            float echo = 0f;
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float noise = Signal.Noise(random);
                body += bodyFactor * (noise - body);
                echo += echoFactor * (noise - echo);
                float crack = noise * Signal.Decay(time, CrackTimeConstant) * CrackLevel;
                float thump = Signal.Sine(ThumpHertz * _pitch, time) * Signal.Decay(time, ThumpTimeConstant) * ThumpLevel;
                float tail = echo * Signal.Decay(time - EchoDelaySeconds, EchoTimeConstant) * EchoLevel;
                samples[index] = crack + (body * Signal.Decay(time, BodyTimeConstant)) + thump + tail;
            }

            return Signal.Normalise(samples, Peak);
        }
    }
}
