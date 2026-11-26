using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Every house either team has ever claimed, shared by both teams. A house is created on its first claim and
    /// then kept for good, so a destroyed house keeps its damage and fortifications for whoever claims it next.
    /// </summary>
    public sealed class HouseLedger
    {
        private readonly FortificationSettings _settings;
        private readonly FortificationSlotLayout _layout;
        private readonly List<ClaimedHouse> _houses = new List<ClaimedHouse>();

        public HouseLedger(FortificationSettings settings, FortificationSlotLayout layout)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        /// <summary>Raised once per building, on its first claim by any team.</summary>
        public event Action<ClaimedHouse> HouseCreated;

        /// <summary>Every house ever claimed, in order of first claim, unoccupied ones included.</summary>
        public IReadOnlyList<ClaimedHouse> Houses => _houses;

        public bool TryGetHouse(GarrisonBuilding building, out ClaimedHouse house)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            house = _houses.FirstOrDefault(candidate => candidate.Building == building);
            return house != null;
        }

        /// <summary>True when the team could claim the building now: it is new, unoccupied or another team's ruin.</summary>
        public bool CanClaim(GarrisonBuilding building, Team team)
        {
            return !TryGetHouse(building, out ClaimedHouse house) || house.IsClaimableBy(team);
        }

        internal ClaimedHouse GetOrCreate(GarrisonBuilding building)
        {
            if (TryGetHouse(building, out ClaimedHouse existing))
            {
                return existing;
            }

            var house = new ClaimedHouse(building, _layout.Create(building), _settings.HouseBaseHealthPoints);
            _houses.Add(house);
            HouseCreated?.Invoke(house);
            return house;
        }
    }
}
