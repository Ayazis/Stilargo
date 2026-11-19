namespace Stilargo.Core.Fog
{
    /// <summary>A position on the ground plane (world X and Z), free of engine types.</summary>
    public readonly struct WorldPoint
    {
        public WorldPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public float X { get; }

        public float Z { get; }
    }
}
