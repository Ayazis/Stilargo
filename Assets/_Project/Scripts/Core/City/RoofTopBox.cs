using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>
    /// A roof known only by its footprint and highest point, as for a building model's bounds. A deck placed on it
    /// stands at the ridge, so it clears the roof whatever the model's shape.
    /// </summary>
    public sealed class RoofTopBox : IRoofSurface
    {
        private readonly WorldPoint _min;
        private readonly WorldPoint _max;
        private readonly float _top;

        public RoofTopBox(WorldPoint min, WorldPoint max, float top)
        {
            if (!(max.X > min.X) || !(max.Z > min.Z))
            {
                throw new ArgumentOutOfRangeException(nameof(max), "The footprint's max corner must lie beyond its min corner.");
            }

            _min = min;
            _max = max;
            _top = top;
        }

        public bool TryGetHeight(WorldPoint point, out float height)
        {
            bool isCovered = point.X >= _min.X && point.X <= _max.X && point.Z >= _min.Z && point.Z <= _max.Z;
            height = isCovered ? _top : 0f;
            return isCovered;
        }
    }
}
