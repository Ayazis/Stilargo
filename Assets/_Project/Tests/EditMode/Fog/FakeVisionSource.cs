using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.Fog
{
    public sealed class FakeVisionSource : IVisionSource
    {
        public FakeVisionSource(float x, float z, float visionRadius)
        {
            Position = new WorldPoint(x, z);
            VisionRadius = visionRadius;
        }

        public WorldPoint Position { get; set; }

        public float VisionRadius { get; set; }

        public float EyeHeightMetres { get; set; } = VisionSettings.DefaultEyeHeightMetres;
    }
}
