using System;
using System.Collections.Generic;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Hud
{
    /// <summary>
    /// What the claim-and-fortify menu shows and does. Before a house is claimed it offers the claim; afterwards
    /// it lists the shop's fortifications and says why a purchase did or did not happen.
    /// </summary>
    public sealed class FortifyMenu
    {
        public const string ClaimHint = "Garrison units in a building to claim it.";
        public const string ClaimLabel = "Claim the house your units are in";

        private static readonly IReadOnlyDictionary<PurchaseResult, string> ResultMessages = new Dictionary<PurchaseResult, string>
        {
            { PurchaseResult.Built, "Built." },
            { PurchaseResult.NoClaimedHouse, "Claim a house first." },
            { PurchaseResult.HouseDestroyed, "The house is destroyed." },
            { PurchaseResult.NoFreeSlot, "No free spot for that." },
            { PurchaseResult.CannotAfford, "Not enough money." },
        };

        private readonly HouseClaim _claim;
        private readonly FortificationShop _shop;
        private readonly ClaimCandidatePicker _picker;

        public FortifyMenu(HouseClaim claim, FortificationShop shop, ClaimCandidatePicker picker)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _picker = picker ?? throw new ArgumentNullException(nameof(picker));
        }

        public bool IsClaimed => _claim.IsClaimed;

        /// <summary>True when some building holds garrisoned units, so claiming would work now.</summary>
        public bool CanClaim => !_claim.IsClaimed && _picker.TryPick(out _);

        public int OfferCount => _shop.Catalogue.Count;

        public string HouseHealthText()
        {
            RequireClaimedHouse();
            return HudText.HouseHealth(_claim.House.CurrentHealth, _claim.House.MaxHealth);
        }

        /// <summary>Claims the building holding the most garrisoned units; false when there is none or one is already claimed.</summary>
        public bool TryClaim()
        {
            return _picker.TryPick(out GarrisonBuilding building) && _claim.TryClaim(building);
        }

        public string OfferLabel(int index)
        {
            FortificationDefinition offer = OfferAt(index);
            return $"{offer.DisplayName} (${offer.CostMoney})";
        }

        public bool IsOfferAvailable(int index)
        {
            return _shop.Check(OfferAt(index)) == PurchaseResult.Built;
        }

        /// <summary>Buys the offer and returns a message for the player.</summary>
        public string Buy(int index)
        {
            return ResultMessages[_shop.TryBuy(OfferAt(index))];
        }

        private FortificationDefinition OfferAt(int index)
        {
            if (index < 0 || index >= _shop.Catalogue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "No such offer.");
            }

            return _shop.Catalogue[index];
        }

        private void RequireClaimedHouse()
        {
            if (!_claim.IsClaimed)
            {
                throw new InvalidOperationException("No house is claimed yet.");
            }
        }
    }
}
