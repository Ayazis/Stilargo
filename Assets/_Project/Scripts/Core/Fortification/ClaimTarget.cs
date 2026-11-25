using System;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Which building the claim-and-fortify menu acts on: the one the squad is garrisoned in. It can be claimed when
    /// it is not yet; once claimed it is the house being fortified and repaired. Without a garrison the menu falls
    /// back to the base.
    /// </summary>
    public sealed class ClaimTarget
    {
        private readonly HouseClaim _claim;
        private readonly ClaimCandidatePicker _picker;

        public ClaimTarget(HouseClaim claim, ClaimCandidatePicker picker)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _picker = picker ?? throw new ArgumentNullException(nameof(picker));
        }

        public bool IsAnyClaimed => _claim.IsClaimed;

        public int ClaimedCount => _claim.Houses.Count;

        /// <summary>True when the squad is garrisoned in a building that is not claimed yet.</summary>
        public bool CanClaim => _picker.TryPick(out GarrisonBuilding building) && !_claim.TryGetHouse(building, out _);

        /// <summary>The claimed house the squad is in, else the base; null while nothing is claimed.</summary>
        public ClaimedHouse SelectedHouse
        {
            get
            {
                if (_picker.TryPick(out GarrisonBuilding building) && _claim.TryGetHouse(building, out ClaimedHouse house))
                {
                    return house;
                }

                return _claim.House;
            }
        }

        public bool IsSelectedTheBase => SelectedHouse != null && SelectedHouse == _claim.House;

        /// <summary>Claims the building holding the most garrisoned units; false when there is none or it is already claimed.</summary>
        public bool TryClaim()
        {
            return _picker.TryPick(out GarrisonBuilding building) && _claim.TryClaim(building);
        }
    }
}
