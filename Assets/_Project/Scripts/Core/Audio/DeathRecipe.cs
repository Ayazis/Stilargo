using System;

namespace Stilargo.Core.Audio
{
    /// <summary>A soldier going down: a falling, breathy groan and then the thud of the body and kit hitting the street.</summary>
    public sealed class DeathRecipe : ISoundRecipe
    {
        private const float DurationSeconds = 0.9f;
        private const float GroanStartHertz = 190f;
        private const float GroanEndHertz = 95f;
        private const float GroanSeconds = 0.45f;
        private const float GroanAttackSeconds = 0.03f;
        private const float VibratoHertz = 7f;
        private const float VibratoDepth = 0.04f;
        private const float BreathCutoffHertz = 900f;
        private const float BreathLevel = 0.35f;
        private const float ThudDelaySeconds = 0.5f;
        private const float ThudHertz = 65f;
        private const float ThudTimeConstant = 0.08f;
        private const float RattleCutoffHertz = 1800f;
        private const float RattleTimeConstant = 0.05f;
        private const float RattleLevel = 0.4f;
        private const float Peak = 0.75f;

        private readonly int _seed;

        public DeathRecipe(int seed)
        {
            _seed = seed;
        }

        public SoundChannel Channel => SoundChannel.Effects;

        public float[] Render(int sampleRate)
        {
            var random = new Random(_seed);
            var samples = new float[Signal.SampleCount(DurationSeconds, sampleRate)];
            float breathFactor = Signal.LowPassFactor(BreathCutoffHertz, sampleRate);
            float rattleFactor = Signal.LowPassFactor(RattleCutoffHertz, sampleRate);
            float breath = 0f;
            float rattle = 0f;
            double phase = 0.0;
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float noise = Signal.Noise(random);
                breath += breathFactor * (noise - breath);
                rattle += rattleFactor * (noise - rattle);

                float progress = Math.Min(1f, time / GroanSeconds);
                float frequency = (GroanStartHertz + ((GroanEndHertz - GroanStartHertz) * progress)) * (1f + (VibratoDepth * Signal.Sine(VibratoHertz, time)));
                phase += 2.0 * Math.PI * frequency / sampleRate;
                float groanEnvelope = time < GroanSeconds ? Math.Min(1f, time / GroanAttackSeconds) * (1f - progress) : 0f;
                float groan = ((float)Math.Sin(phase) + (breath * BreathLevel)) * groanEnvelope;

                float sinceThud = time - ThudDelaySeconds;
                float thud = sinceThud < 0f ? 0f : (Signal.Sine(ThudHertz, sinceThud) * Signal.Decay(sinceThud, ThudTimeConstant))
                    + (rattle * Signal.Decay(sinceThud, RattleTimeConstant) * RattleLevel);
                samples[index] = groan + thud;
            }

            return Signal.Normalise(samples, Peak);
        }
    }
}
