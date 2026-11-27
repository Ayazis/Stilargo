using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>
    /// The top surface of one hip roof built by <see cref="HipRoofBuilder"/>. All four slopes share one pitch, so the
    /// height anywhere is the pitch times the distance to the nearest eave, capped at the ridge.
    /// </summary>
    public sealed class HipRoofSurface
    {
        private const float Half = 0.5f;

        private readonly float _centreX;
        private readonly float _centreZ;
        private readonly float _eavesHeight;
        private readonly float _halfSizeX;
        private readonly float _halfSizeZ;
        private readonly float _rise;
        private readonly float _risePerMetre;

        /// <param name="sizeX">Roof size along world X, eaves overhang included.</param>
        /// <param name="sizeZ">Roof size along world Z, eaves overhang included.</param>
        public HipRoofSurface(WorldPoint centre, float eavesHeight, float sizeX, float sizeZ, float rise)
        {
            if (!(sizeX > 0f) || !(sizeZ > 0f) || !(rise > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(rise), $"Roof size ({sizeX} x {sizeZ}) and rise ({rise}) must be positive.");
            }

            _centreX = centre.X;
            _centreZ = centre.Z;
            _eavesHeight = eavesHeight;
            _halfSizeX = sizeX * Half;
            _halfSizeZ = sizeZ * Half;
            _rise = rise;
            _risePerMetre = rise / Math.Min(_halfSizeX, _halfSizeZ);
        }

        public bool TryGetHeight(WorldPoint point, out float height)
        {
            float toEaveX = _halfSizeX - Math.Abs(point.X - _centreX);
            float toEaveZ = _halfSizeZ - Math.Abs(point.Z - _centreZ);
            float toNearestEave = Math.Min(toEaveX, toEaveZ);
            if (toNearestEave < 0f)
            {
                height = 0f;
                return false;
            }

            height = _eavesHeight + Math.Min(_rise, toNearestEave * _risePerMetre);
            return true;
        }
    }
}
