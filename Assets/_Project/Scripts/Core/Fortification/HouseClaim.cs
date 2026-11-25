using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Fog;
using Stilargo.Core.Garrison;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// The buildings the player has claimed. Claiming is free and any number of buildings can be claimed. The first
    /// claim is the base: scavengers deliver loot to its entrance instead of the starting drop-off, and losing it
    /// loses the match.
    /// </summary>
    public sealed class HouseClaim
    {
        private readonly FortificationSettings _settings;
        private readonly FortificationSlotLayout _layout;
        private readonly DropOff _dropOff;
        private readonly List<ClaimedHouse> _houses = new List<ClaimedHouse>();

        public HouseClaim(FortificationSettings settings, FortificationSlotLayout layout, DropOff dropOff)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _dropOff = dropOff ?? throw new ArgumentNullException(nameof(dropOff));
        }

        /// <summary>Raised for every new claim, the base included.</summary>
        public event Action<ClaimedHouse> Claimed;

        /// <summary>The base (the first claimed house), or null until <see cref="TryClaim"/> succeeds.</summary>
        public ClaimedHouse House => _houses.Count == 0 ? null : _houses[0];

        /// <summary>Every claimed house in claim order; the base comes first.</summary>
        public IReadOnlyList<ClaimedHouse> Houses => _houses;

        public bool IsClaimed => _houses.Count > 0;

        public bool TryGetHouse(GarrisonBuilding building, out ClaimedHouse house)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            house = _houses.FirstOrDefault(candidate => candidate.Building == building);
            return house != null;
        }

        /// <summary>Claims the building; false when it is already claimed.</summary>
        public bool TryClaim(GarrisonBuilding building)
        {
            if (TryGetHouse(building, out _))
            {
                return false;
            }

            var house = new ClaimedHouse(building, _layout.Create(building), _settings.HouseBaseHealthPoints);
            _houses.Add(house);
            if (house == House)
            {
                _dropOff.Relocate(new WorldPoint(building.Entrance.X, building.Entrance.Z));
            }

            Claimed?.Invoke(house);
            return true;
        }
    }
}
