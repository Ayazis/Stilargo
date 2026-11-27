using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// The houses one team owns. Claiming is free and any number of houses can be owned. A team can claim a house
    /// nobody owns or one the other team has let fall to ruin. A destroyed house becomes unoccupied and leaves its
    /// owner. The oldest owned house is the base.
    /// </summary>
    public sealed class HouseClaim
    {
        private readonly HouseLedger _ledger;
        private readonly List<ClaimedHouse> _houses = new List<ClaimedHouse>();

        public HouseClaim(Team team, HouseLedger ledger)
        {
            Team = team;
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
        }

        /// <summary>Raised for every new claim, the base included.</summary>
        public event Action<ClaimedHouse> Claimed;

        /// <summary>Raised when an owned house is destroyed or taken by the other team.</summary>
        public event Action<ClaimedHouse> Lost;

        public Team Team { get; }

        /// <summary>Every house either team has claimed, so callers can look up a house this team does not own.</summary>
        public HouseLedger Ledger => _ledger;

        /// <summary>The base (the oldest owned house), or null while the team owns none.</summary>
        public ClaimedHouse House => _houses.Count == 0 ? null : _houses[0];

        /// <summary>Every owned house in claim order; the base comes first.</summary>
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

        /// <summary>True when the building is not this team's and is free, or a ruin, to take.</summary>
        public bool CanClaim(GarrisonBuilding building)
        {
            return _ledger.CanClaim(building, Team);
        }

        /// <summary>Claims the building; false when this team owns it already or the other team holds it standing.</summary>
        public bool TryClaim(GarrisonBuilding building)
        {
            if (!CanClaim(building))
            {
                return false;
            }

            ClaimedHouse house = _ledger.GetOrCreate(building);
            house.TakeOver(Team);
            house.OwnerChanged += HandleOwnerChanged;
            _houses.Add(house);
            Claimed?.Invoke(house);
            return true;
        }

        private void HandleOwnerChanged(ClaimedHouse house)
        {
            if (house.IsOwnedBy(Team))
            {
                return;
            }

            house.OwnerChanged -= HandleOwnerChanged;
            _houses.Remove(house);
            Lost?.Invoke(house);
        }
    }
}
