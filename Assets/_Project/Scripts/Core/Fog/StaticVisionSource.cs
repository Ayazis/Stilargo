using System;

namespace Stilargo.Core.Fog
{
    /// <summary>Reveals fog around a point that never moves, e.g. a turret on the claimed house.</summary>
    public sealed class StaticVisionSource : IVisionSource
    {
        public StaticVisionSource(WorldPoint position, float visionRadius)
        {
            if (!(visionRadius > 0f) || float.IsInfinity(visionRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(visionRadius), visionRadius, "Vision radius must be positive and finite.");
            }

            Position = position;
            VisionRadius = visionRadius;
        }

        public WorldPoint Position { get; }

        public float VisionRadius { get; }
    }
}
