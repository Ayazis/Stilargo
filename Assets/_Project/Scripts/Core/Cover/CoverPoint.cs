using System;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Cover
{
    /// <summary>A spot on the road against a building wall where a unit takes cover.</summary>
    public sealed class CoverPoint
    {
        private const float UnitLengthTolerance = 1e-3f;

        public CoverPoint(Vector3 position, Vector3 normal, CombatProtection protection)
        {
            if (Math.Abs(normal.Length() - 1f) > UnitLengthTolerance)
            {
                throw new ArgumentException("Normal must be a unit vector.", nameof(normal));
            }

            Position = position;
            Normal = normal;
            Protection = protection;
        }

        /// <summary>World position on the ground, just off the wall.</summary>
        public Vector3 Position { get; }

        /// <summary>Unit vector pointing from the wall out into the street.</summary>
        public Vector3 Normal { get; }

        public CombatProtection Protection { get; }
    }
}
