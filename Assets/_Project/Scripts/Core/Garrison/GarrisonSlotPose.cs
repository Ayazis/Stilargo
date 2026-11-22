using System;
using System.Numerics;

namespace Stilargo.Core.Garrison
{
    /// <summary>Where a garrison slot's unit stands and the unit vector it looks along.</summary>
    public readonly struct GarrisonSlotPose
    {
        private const float MinimumFacingLength = 1e-6f;

        public GarrisonSlotPose(Vector3 position, Vector3 facing)
        {
            if (facing.Length() < MinimumFacingLength)
            {
                throw new ArgumentException("Facing must not be zero.", nameof(facing));
            }

            Position = position;
            Facing = Vector3.Normalize(facing);
        }

        public Vector3 Position { get; }

        public Vector3 Facing { get; }
    }
}
