using System;
using System.Linq;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Hud
{
    /// <summary>The one-line description of the building the claim-and-fortify panel is looking at.</summary>
    public static class BuildingInfo
    {
        public const string Yours = "Yours";
        public const string Rival = "Rival";
        public const string Unoccupied = "Unoccupied";
        public const string Ruin = "Ruin";
        public const string RivalHint = "Held by the rival. Bring it down to take it.";

        private const string Separator = " · ";

        public static string Describe(ClaimTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            GarrisonBuilding building = target.FocusedBuilding;
            if (building == null)
            {
                return string.Empty;
            }

            int occupied = building.Slots.Count(slot => slot.IsOccupied);
            string garrison = $"{occupied}/{building.Slots.Count} garrisoned";
            string name = building.Plot.Use.ToString();
            if (!target.TryGetFocusedRecord(out ClaimedHouse house))
            {
                return string.Join(Separator, name, Unoccupied, garrison);
            }

            string health = $"{house.CurrentHealth:0}/{house.MaxHealth:0} HP";
            return string.Join(Separator, name, OwnerOf(house, target.Team), health, garrison);
        }

        /// <summary>True when the focused building is a standing rival house, which cannot be claimed until it falls.</summary>
        public static bool IsHeldByRival(ClaimTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return target.TryGetFocusedRecord(out ClaimedHouse house) && house.IsAlive && !house.IsUnoccupied && !house.IsOwnedBy(target.Team);
        }

        private static string OwnerOf(ClaimedHouse house, Team playerTeam)
        {
            if (!house.IsAlive)
            {
                return Ruin;
            }

            if (house.IsUnoccupied)
            {
                return Unoccupied;
            }

            return house.IsOwnedBy(playerTeam) ? Yours : Rival;
        }
    }
}
