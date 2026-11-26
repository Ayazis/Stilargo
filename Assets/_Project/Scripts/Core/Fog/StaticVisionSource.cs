using System;

namespace Stilargo.Core.Fog
{
    /// <summary>Reveals fog around a point that never moves, e.g. a turret on the claimed house.</summary>
    public sealed class StaticVisionSource : IVisionSource
    {
        public StaticVisionSource(WorldPoint position, float visionRadius)
            : this(position, visionRadius, VisionSettings.DefaultEyeHeightMetres)
        {
        }

        public StaticVisionSource(WorldPoint position, float visionRadius, float eyeHeightMetres)
        {
            if (!(visionRadius > 0f) || float.IsInfinity(visionRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(visionRadius), visionRadius, "Vision radius must be positive and finite.");
            }

            if (!(eyeHeightMetres >= 0f) || float.IsInfinity(eyeHeightMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(eyeHeightMetres), eyeHeightMetres, "Eye height must be a non-negative number.");
            }

            Position = position;
            VisionRadius = visionRadius;
            EyeHeightMetres = eyeHeightMetres;
        }

        public WorldPoint Position { get; }

        public float VisionRadius { get; }

        public float EyeHeightMetres { get; }
    }
}
