using System;
using System.Collections.Generic;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Sells fortifications for the claimed house. A purchase fills the first free slot of the right kind and
    /// takes the money only when it can actually be built.
    /// </summary>
    public sealed class FortificationShop
    {
        private readonly HouseClaim _claim;
        private readonly Wallet _wallet;

        public FortificationShop(HouseClaim claim, Wallet wallet, IReadOnlyList<FortificationDefinition> catalogue)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
        }

        public IReadOnlyList<FortificationDefinition> Catalogue { get; }

        /// <summary>What <see cref="TryBuy"/> would return, without buying.</summary>
        public PurchaseResult Check(FortificationDefinition fortification)
        {
            return Check(fortification, out _);
        }

        public PurchaseResult TryBuy(FortificationDefinition fortification)
        {
            PurchaseResult result = Check(fortification, out FortificationSlot slot);
            if (result != PurchaseResult.Built)
            {
                return result;
            }

            _wallet.TrySpend(fortification.CostMoney);
            _claim.House.Build(slot, fortification);
            return PurchaseResult.Built;
        }

        private PurchaseResult Check(FortificationDefinition fortification, out FortificationSlot slot)
        {
            if (fortification == null)
            {
                throw new ArgumentNullException(nameof(fortification));
            }

            slot = null;
            if (!_claim.IsClaimed)
            {
                return PurchaseResult.NoClaimedHouse;
            }

            if (!_claim.House.IsAlive)
            {
                return PurchaseResult.HouseDestroyed;
            }

            if (!_claim.House.TryFindFreeSlot(fortification.SlotKind, out slot))
            {
                return PurchaseResult.NoFreeSlot;
            }

            return _wallet.CanAfford(fortification.CostMoney) ? PurchaseResult.Built : PurchaseResult.CannotAfford;
        }
    }
}
