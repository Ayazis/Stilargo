using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Cover
{
    /// <summary>Finds the cover point a unit is standing in, using a ground-plane bucket grid.</summary>
    public sealed class CoverMap
    {
        private const int NeighbourBucketReach = 1;

        private readonly float _captureRadius;
        private readonly Dictionary<(int X, int Z), List<CoverPoint>> _buckets = new Dictionary<(int X, int Z), List<CoverPoint>>();

        public CoverMap(IReadOnlyList<CoverPoint> points, CoverSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Points = points ?? throw new ArgumentNullException(nameof(points));
            // Buckets as wide as the radius mean every candidate lies in the 3x3 buckets around a query.
            _captureRadius = settings.CaptureRadiusMetres;
            foreach (CoverPoint point in points)
            {
                AddToBucket(point);
            }
        }

        public IReadOnlyList<CoverPoint> Points { get; }

        /// <summary>Nearest cover point within the capture radius on the ground plane (Y ignored).</summary>
        public bool TryFindCoverAt(Vector3 position, out CoverPoint point)
        {
            point = null;
            float bestDistanceSquared = _captureRadius * _captureRadius;
            (int X, int Z) centre = BucketOf(position);
            for (int dz = -NeighbourBucketReach; dz <= NeighbourBucketReach; dz++)
            {
                for (int dx = -NeighbourBucketReach; dx <= NeighbourBucketReach; dx++)
                {
                    if (!_buckets.TryGetValue((centre.X + dx, centre.Z + dz), out List<CoverPoint> bucket))
                    {
                        continue;
                    }

                    foreach (CoverPoint candidate in bucket)
                    {
                        float distanceSquared = GroundDistanceSquared(position, candidate.Position);
                        if (distanceSquared <= bestDistanceSquared)
                        {
                            bestDistanceSquared = distanceSquared;
                            point = candidate;
                        }
                    }
                }
            }

            return point != null;
        }

        /// <summary>The cover bonus at a position: the point's protection, or no bonus outside cover.</summary>
        public PositionBonus BonusAt(Vector3 position)
        {
            if (!TryFindCoverAt(position, out CoverPoint point))
            {
                return PositionBonus.None;
            }

            return new PositionBonus(0f, 0f, point.Protection);
        }

        private void AddToBucket(CoverPoint point)
        {
            if (point == null)
            {
                throw new ArgumentException("Cover points must not contain null.", nameof(point));
            }

            (int X, int Z) key = BucketOf(point.Position);
            if (!_buckets.TryGetValue(key, out List<CoverPoint> bucket))
            {
                bucket = new List<CoverPoint>();
                _buckets.Add(key, bucket);
            }

            bucket.Add(point);
        }

        private (int X, int Z) BucketOf(Vector3 position)
        {
            return ((int)Math.Floor(position.X / _captureRadius), (int)Math.Floor(position.Z / _captureRadius));
        }

        private static float GroundDistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return (dx * dx) + (dz * dz);
        }
    }
}
