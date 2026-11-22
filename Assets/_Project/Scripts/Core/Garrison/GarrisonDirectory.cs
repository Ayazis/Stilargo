using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.City;

namespace Stilargo.Core.Garrison
{
    /// <summary>Looks up enterable buildings by world position and by who is inside them.</summary>
    public sealed class GarrisonDirectory
    {
        private readonly List<GarrisonBuilding> _buildings;
        private readonly float _cellSize;

        public GarrisonDirectory(IReadOnlyList<GarrisonBuilding> buildings, float cellSize)
        {
            if (buildings == null)
            {
                throw new ArgumentNullException(nameof(buildings));
            }

            if (buildings.Any(building => building == null))
            {
                throw new ArgumentException("Buildings must not contain null.", nameof(buildings));
            }

            if (!(cellSize > 0f) || float.IsInfinity(cellSize))
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be positive and finite.");
            }

            _buildings = buildings.ToList();
            _cellSize = cellSize;
        }

        public IReadOnlyList<GarrisonBuilding> Buildings => _buildings;

        /// <summary>Finds the building whose plot footprint contains the world point (metres, city corner at origin).</summary>
        public bool TryFindBuildingAt(float worldX, float worldZ, out GarrisonBuilding building)
        {
            GridPosition cell = new GridPosition(
                (int)Math.Floor(worldX / _cellSize),
                (int)Math.Floor(worldZ / _cellSize));
            building = _buildings.FirstOrDefault(candidate => candidate.Plot.Area.Contains(cell));
            return building != null;
        }

        public bool TryFindOccupiedBuilding(object occupant, out GarrisonBuilding building)
        {
            if (occupant == null)
            {
                throw new ArgumentNullException(nameof(occupant));
            }

            building = _buildings.FirstOrDefault(candidate => candidate.Contains(occupant));
            return building != null;
        }

        /// <summary>Releases the occupant from every building it is in; true if it was in any.</summary>
        public bool ReleaseEverywhere(object occupant)
        {
            if (occupant == null)
            {
                throw new ArgumentNullException(nameof(occupant));
            }

            bool wasReleased = false;
            foreach (GarrisonBuilding building in _buildings)
            {
                wasReleased |= building.Release(occupant);
            }

            return wasReleased;
        }
    }
}
