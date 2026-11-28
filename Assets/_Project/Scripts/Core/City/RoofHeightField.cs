using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>
    /// A building model's roof as a grid of heights baked from its mesh, placed in the world at a position and a
    /// quarter-turn facing, so a turret deck rests on the real tiles instead of the model's highest point.
    /// </summary>
    public sealed class RoofHeightField : IRoofSurface
    {
        private const double QuarterTurnRadians = Math.PI / 2d;

        private readonly RoofGrid _grid;
        private readonly WorldPoint _origin;
        private readonly float _baseHeight;
        private readonly float _cos;
        private readonly float _sin;

        /// <param name="origin">Where the model's own origin stands, on the ground.</param>
        /// <param name="baseHeight">The world height of the model's origin.</param>
        public RoofHeightField(RoofGrid grid, WorldPoint origin, float baseHeight, Facing facing)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
            _origin = origin;
            _baseHeight = baseHeight;
            double angle = (int)facing * QuarterTurnRadians;
            _cos = (float)Math.Round(Math.Cos(angle));
            _sin = (float)Math.Round(Math.Sin(angle));
        }

        public bool TryGetHeight(WorldPoint point, out float height)
        {
            // Turned by the facing, the model's local (x, z) lands at world (x cos + z sin, z cos - x sin); this undoes it.
            float dx = point.X - _origin.X;
            float dz = point.Z - _origin.Z;
            float localX = (dx * _cos) - (dz * _sin);
            float localZ = (dx * _sin) + (dz * _cos);
            if (!_grid.TryGetHeight(localX, localZ, out float local))
            {
                height = 0f;
                return false;
            }

            height = _baseHeight + local;
            return true;
        }
    }
}
