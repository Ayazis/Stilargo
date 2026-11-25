using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Chooses which building a claim takes: the one holding the most garrisoned units, so the player claims a
    /// house by moving the squad into it. On a tie the building listed first wins.
    /// </summary>
    public sealed class ClaimCandidatePicker
    {
        private readonly IReadOnlyList<GarrisonBuilding> _buildings;

        public ClaimCandidatePicker(IReadOnlyList<GarrisonBuilding> buildings)
        {
            _buildings = buildings ?? throw new ArgumentNullException(nameof(buildings));
        }

        public bool TryPick(out GarrisonBuilding building)
        {
            building = null;
            int mostOccupants = 0;
            foreach (GarrisonBuilding candidate in _buildings)
            {
                int occupants = candidate.Slots.Count(slot => slot.IsOccupied);
                if (occupants > mostOccupants)
                {
                    building = candidate;
                    mostOccupants = occupants;
                }
            }

            return building != null;
        }
    }
}
