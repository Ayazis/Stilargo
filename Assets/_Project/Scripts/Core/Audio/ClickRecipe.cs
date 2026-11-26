namespace Stilargo.Core.Audio
{
    /// <summary>A soft, short tick for HUD buttons.</summary>
    public sealed class ClickRecipe : ISoundRecipe
    {
        private const float DurationSeconds = 0.06f;
        private const float ToneHertz = 1700f;
        private const float OvertoneHertz = 3400f;
        private const float OvertoneLevel = 0.3f;
        private const float TimeConstant = 0.009f;
        private const float Peak = 0.6f;

        public SoundChannel Channel => SoundChannel.Interface;

        public float[] Render(int sampleRate)
        {
            var samples = new float[Signal.SampleCount(DurationSeconds, sampleRate)];
            for (int index = 0; index < samples.Length; index++)
            {
                float time = index / (float)sampleRate;
                float tone = Signal.Sine(ToneHertz, time) + (Signal.Sine(OvertoneHertz, time) * OvertoneLevel);
                samples[index] = tone * Signal.Decay(time, TimeConstant);
            }

            return Signal.Normalise(samples, Peak);
        }
    }
}
