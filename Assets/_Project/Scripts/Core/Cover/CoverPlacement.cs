using System;

namespace Stilargo.Core.Cover
{
    /// <summary>Where cover points are placed along building walls.</summary>
    public sealed class CoverPlacement
    {
        public const float DefaultWallOffsetMetres = 0.8f;
        public const int DefaultPointsPerEdge = 1;

        public CoverPlacement(float wallOffsetMetres, int pointsPerEdge)
        {
            if (!(wallOffsetMetres > 0f) || float.IsInfinity(wallOffsetMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(wallOffsetMetres), wallOffsetMetres, "Wall offset must be a positive finite number.");
            }

            if (pointsPerEdge <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pointsPerEdge), pointsPerEdge, "Points per edge must be positive.");
            }

            WallOffsetMetres = wallOffsetMetres;
            PointsPerEdge = pointsPerEdge;
        }

        public static CoverPlacement Default => new CoverPlacement(DefaultWallOffsetMetres, DefaultPointsPerEdge);

        /// <summary>Distance from the building wall into the road cell, in metres.</summary>
        public float WallOffsetMetres { get; }

        /// <summary>Cover points spread evenly along each road-cell edge that touches a building.</summary>
        public int PointsPerEdge { get; }
    }
}
