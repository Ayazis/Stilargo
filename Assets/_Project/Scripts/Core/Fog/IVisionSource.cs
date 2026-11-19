namespace Stilargo.Core.Fog
{
    public interface IVisionSource
    {
        WorldPoint Position { get; }

        float VisionRadius { get; }
    }
}
