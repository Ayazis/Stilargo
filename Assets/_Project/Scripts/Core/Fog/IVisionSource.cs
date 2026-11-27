namespace Stilargo.Core.Fog
{
    public interface IVisionSource
    {
        WorldPoint Position { get; }

        float VisionRadius { get; }

        /// <summary>Height of the eyes above the ground in metres; obstacles at least this tall block the view.</summary>
        float EyeHeightMetres { get; }
    }
}
