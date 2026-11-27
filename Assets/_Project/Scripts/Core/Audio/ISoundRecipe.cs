namespace Stilargo.Core.Audio
{
    /// <summary>Synthesises one sound as mono samples in [-1, 1].</summary>
    public interface ISoundRecipe
    {
        SoundChannel Channel { get; }

        float[] Render(int sampleRate);
    }
}
