using System;
using Stilargo.Core.Fog;
using Stilargo.Core.Garrison;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// The one-time choice of which building becomes the player's base. Claiming is free; once claimed,
    /// scavengers deliver loot to the house's entrance instead of the starting drop-off.
    /// </summary>
    public sealed class HouseClaim
    {
        private readonly FortificationSettings _settings;
        private readonly FortificationSlotLayout _layout;
        private readonly DropOff _dropOff;

        public HouseClaim(FortificationSettings settings, FortificationSlotLayout layout, DropOff dropOff)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _dropOff = dropOff ?? throw new ArgumentNullException(nameof(dropOff));
        }

        public event Action<ClaimedHouse> Claimed;

        /// <summary>The claimed house, or null until <see cref="TryClaim"/> succeeds.</summary>
        public ClaimedHouse House { get; private set; }

        public bool IsClaimed => House != null;

        /// <summary>Claims the building; false when a house is already claimed.</summary>
        public bool TryClaim(GarrisonBuilding building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (IsClaimed)
            {
                return false;
            }

            House = new ClaimedHouse(building, _layout.Create(building), _settings.HouseBaseHealthPoints);
            _dropOff.Relocate(new WorldPoint(building.Entrance.X, building.Entrance.Z));
            Claimed?.Invoke(House);
            return true;
        }
    }
}
