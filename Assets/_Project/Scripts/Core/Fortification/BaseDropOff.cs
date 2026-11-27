using System;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Keeps the drop-off at the base's entrance, so scavengers deliver loot home. When the base falls, the drop-off
    /// moves to the next owned house; with none left it stays where it was.
    /// </summary>
    public sealed class BaseDropOff
    {
        private readonly HouseClaim _claim;
        private readonly DropOff _dropOff;
        private ClaimedHouse _base;

        public BaseDropOff(HouseClaim claim, DropOff dropOff)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _dropOff = dropOff ?? throw new ArgumentNullException(nameof(dropOff));
            _claim.Claimed += _ => FollowBase();
            _claim.Lost += _ => FollowBase();
            FollowBase();
        }

        private void FollowBase()
        {
            ClaimedHouse house = _claim.House;
            if (house == null || house == _base)
            {
                return;
            }

            _base = house;
            _dropOff.Relocate(new WorldPoint(house.Building.Entrance.X, house.Building.Entrance.Z));
        }
    }
}
