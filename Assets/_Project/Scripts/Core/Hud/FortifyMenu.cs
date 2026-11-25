using System;
using System.Collections.Generic;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Hud
{
    /// <summary>
    /// What the claim-and-fortify menu shows and does. It offers a claim whenever the squad stands in an unclaimed
    /// building; once something is claimed it lists the shop's fortifications for the selected house and says why a
    /// purchase did or did not happen.
    /// </summary>
    public sealed class FortifyMenu
    {
        public const string ClaimHint = "Garrison units in a building to claim it.";
        public const string ClaimLabel = "Claim the building your units are in";

        private static readonly IReadOnlyDictionary<PurchaseResult, string> ResultMessages = new Dictionary<PurchaseResult, string>
        {
            { PurchaseResult.Built, "Built." },
            { PurchaseResult.NoClaimedHouse, "Claim a house first." },
            { PurchaseResult.HouseDestroyed, "The house is destroyed." },
            { PurchaseResult.NoFreeSlot, "No free spot for that." },
            { PurchaseResult.CannotAfford, "Not enough money." },
        };

        private readonly ClaimTarget _target;
        private readonly FortificationShop _shop;

        public FortifyMenu(ClaimTarget target, FortificationShop shop, UpkeepMenu upkeep)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            Upkeep = upkeep ?? throw new ArgumentNullException(nameof(upkeep));
        }

        public UpkeepMenu Upkeep { get; }

        public bool IsClaimed => _target.IsAnyClaimed;

        /// <summary>True when the squad is garrisoned in an unclaimed building, so claiming would work now.</summary>
        public bool CanClaim => _target.CanClaim;

        public int OfferCount => _shop.Catalogue.Count;

        public string HouseHealthText()
        {
            ClaimedHouse house = RequireSelectedHouse();
            string houseName = _target.IsSelectedTheBase ? HudText.BaseName : HudText.HouseName;
            return HudText.HouseHealth(houseName, house.CurrentHealth, house.MaxHealth);
        }

        /// <summary>Claims the building holding the most garrisoned units; false when there is none or it is already claimed.</summary>
        public bool TryClaim()
        {
            return _target.TryClaim();
        }

        public string OfferLabel(int index)
        {
            FortificationDefinition offer = OfferAt(index);
            return $"{offer.DisplayName} (${offer.CostMoney})";
        }

        public bool IsOfferAvailable(int index)
        {
            return _shop.Check(_target.SelectedHouse, OfferAt(index)) == PurchaseResult.Built;
        }

        /// <summary>Buys the offer for the selected house and returns a message for the player.</summary>
        public string Buy(int index)
        {
            return ResultMessages[_shop.TryBuy(_target.SelectedHouse, OfferAt(index))];
        }

        private FortificationDefinition OfferAt(int index)
        {
            if (index < 0 || index >= _shop.Catalogue.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "No such offer.");
            }

            return _shop.Catalogue[index];
        }

        private ClaimedHouse RequireSelectedHouse()
        {
            ClaimedHouse house = _target.SelectedHouse;
            if (house == null)
            {
                throw new InvalidOperationException("No house is claimed yet.");
            }

            return house;
        }
    }
}
