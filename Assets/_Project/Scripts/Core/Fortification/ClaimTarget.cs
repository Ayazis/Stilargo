using System;
using System.Linq;
using Stilargo.Core.Garrison;
using Stilargo.Core.Selection;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Which building the claim-and-fortify menu acts on: the one the player clicked, else the one the squad is
    /// garrisoned in. It can be claimed while units are garrisoned in it and it is unoccupied or an enemy ruin; once
    /// claimed it is the house being fortified and repaired. With nothing clicked or garrisoned the menu falls back to the base.
    /// </summary>
    public sealed class ClaimTarget
    {
        private readonly HouseClaim _claim;
        private readonly ClaimCandidatePicker _picker;
        private readonly BuildingSelection _selection;

        public ClaimTarget(HouseClaim claim, ClaimCandidatePicker picker)
            : this(claim, picker, new BuildingSelection())
        {
        }

        public ClaimTarget(HouseClaim claim, ClaimCandidatePicker picker, BuildingSelection selection)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _picker = picker ?? throw new ArgumentNullException(nameof(picker));
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
        }

        public Team Team => _claim.Team;

        public bool IsAnyClaimed => _claim.IsClaimed;

        public int ClaimedCount => _claim.Houses.Count;

        /// <summary>The clicked building, else the one holding the most garrisoned units; null when there is neither.</summary>
        public GarrisonBuilding FocusedBuilding
        {
            get
            {
                if (_selection.HasSelection)
                {
                    return _selection.Selected;
                }

                return _picker.TryPick(out GarrisonBuilding building) ? building : null;
            }
        }

        /// <summary>True when the player clicked a building, so the menu describes it even if it is not theirs.</summary>
        public bool IsBuildingSelected => _selection.HasSelection;

        /// <summary>True when units are garrisoned in the focused building and it is unoccupied or an enemy ruin.</summary>
        public bool CanClaim
        {
            get
            {
                GarrisonBuilding building = FocusedBuilding;
                return building != null && IsGarrisoned(building) && _claim.CanClaim(building);
            }
        }

        /// <summary>
        /// The player's house the menu manages: the focused building when it is theirs, else the base. A clicked
        /// building that is not theirs has none, so the menu never acts on a house the player is not looking at.
        /// </summary>
        public ClaimedHouse SelectedHouse
        {
            get
            {
                GarrisonBuilding building = FocusedBuilding;
                if (building != null && _claim.TryGetHouse(building, out ClaimedHouse house))
                {
                    return house;
                }

                return IsBuildingSelected ? null : _claim.House;
            }
        }

        public bool IsSelectedTheBase => SelectedHouse != null && SelectedHouse == _claim.House;

        /// <summary>The focused building's house record whoever owns it; false when nobody ever claimed it.</summary>
        public bool TryGetFocusedRecord(out ClaimedHouse house)
        {
            GarrisonBuilding building = FocusedBuilding;
            house = null;
            return building != null && _claim.Ledger.TryGetHouse(building, out house);
        }

        /// <summary>Claims the focused building; false when no units hold it or it cannot be claimed.</summary>
        public bool TryClaim()
        {
            return CanClaim && _claim.TryClaim(FocusedBuilding);
        }

        private static bool IsGarrisoned(GarrisonBuilding building)
        {
            return building.Slots.Any(slot => slot.IsOccupied);
        }
    }
}
