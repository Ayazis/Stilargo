using System;
using System.Linq;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Rival
{
    /// <summary>The houses the rival cares about: the player's to raid, its own, and fallen ones it can confiscate.</summary>
    public sealed class RivalHouses
    {
        private readonly HouseLedger _ledger;

        public RivalHouses(HouseLedger ledger, HouseClaim playerHouses, HouseClaim ownHouses)
        {
            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            PlayerHouses = playerHouses ?? throw new ArgumentNullException(nameof(playerHouses));
            OwnHouses = ownHouses ?? throw new ArgumentNullException(nameof(ownHouses));
            if (playerHouses.Team == ownHouses.Team)
            {
                throw new ArgumentException("The rival's houses must belong to another team than the player's.", nameof(ownHouses));
            }
        }

        public HouseClaim PlayerHouses { get; }

        public HouseClaim OwnHouses { get; }

        public bool IsPlayerHouseStanding => PlayerHouses.Houses.Any(house => house.IsAlive);

        /// <summary>The player's oldest standing house, base first; false when none stands.</summary>
        public bool TryFindRaidTarget(out ClaimedHouse house)
        {
            house = PlayerHouses.Houses.FirstOrDefault(candidate => candidate.IsAlive);
            return house != null;
        }

        /// <summary>True while the house belongs to the player and still stands, so a raid on it goes on.</summary>
        public bool IsRaidable(ClaimedHouse house)
        {
            return house != null && house.IsAlive && house.IsOwnedBy(PlayerHouses.Team);
        }

        /// <summary>True while the rival could claim the house: it is unoccupied or a ruin.</summary>
        public bool IsConfiscatable(ClaimedHouse house)
        {
            return house != null && house.IsClaimableBy(OwnHouses.Team);
        }

        /// <summary>
        /// The confiscatable house nearest to <paramref name="from"/>. Only houses someone has claimed before count,
        /// so the rival takes fallen houses instead of grabbing the whole city.
        /// </summary>
        public bool TryFindNearestToConfiscate(Vector3 from, out ClaimedHouse house)
        {
            house = _ledger.Houses
                .Where(IsConfiscatable)
                .OrderBy(candidate => GroundDistance.Between(from, candidate.Position))
                .FirstOrDefault();
            return house != null;
        }
    }
}
