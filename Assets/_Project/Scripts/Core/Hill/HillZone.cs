using System;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Hill
{
    /// <summary>A circle on the ground. Height is ignored, so units on stairs or slopes still count.</summary>
    public sealed class HillZone
    {
        public HillZone(Vector3 center, float radiusMetres)
        {
            if (!(radiusMetres > 0f) || float.IsInfinity(radiusMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(radiusMetres), radiusMetres, "Hill radius must be positive and finite.");
            }

            Center = center;
            RadiusMetres = radiusMetres;
        }

        public Vector3 Center { get; }

        public float RadiusMetres { get; }

        public bool Contains(Vector3 position)
        {
            return GroundDistance.Between(Center, position) <= RadiusMetres;
        }
    }
}
