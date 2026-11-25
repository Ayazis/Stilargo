using System;
using System.Collections.Generic;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Sells fortifications for a claimed house. A purchase fills the house's first free slot of the right kind and
    /// takes the money only when it can actually be built.
    /// </summary>
    public sealed class FortificationShop
    {
        private readonly Wallet _wallet;

        public FortificationShop(Wallet wallet, IReadOnlyList<FortificationDefinition> catalogue)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));
        }

        public IReadOnlyList<FortificationDefinition> Catalogue { get; }

        /// <summary>What <see cref="TryBuy"/> would return, without buying.</summary>
        /// <param name="house">The house to build on, or null when none is claimed.</param>
        public PurchaseResult Check(ClaimedHouse house, FortificationDefinition fortification)
        {
            return Check(house, fortification, out _);
        }

        /// <param name="house">The house to build on, or null when none is claimed.</param>
        public PurchaseResult TryBuy(ClaimedHouse house, FortificationDefinition fortification)
        {
            PurchaseResult result = Check(house, fortification, out FortificationSlot slot);
            if (result != PurchaseResult.Built)
            {
                return result;
            }

            _wallet.TrySpend(fortification.CostMoney);
            house.Build(slot, fortification);
            return PurchaseResult.Built;
        }

        private PurchaseResult Check(ClaimedHouse house, FortificationDefinition fortification, out FortificationSlot slot)
        {
            if (fortification == null)
            {
                throw new ArgumentNullException(nameof(fortification));
            }

            slot = null;
            if (house == null)
            {
                return PurchaseResult.NoClaimedHouse;
            }

            if (!house.IsAlive)
            {
                return PurchaseResult.HouseDestroyed;
            }

            if (!house.TryFindFreeSlot(fortification.SlotKind, out slot))
            {
                return PurchaseResult.NoFreeSlot;
            }

            return _wallet.CanAfford(fortification.CostMoney) ? PurchaseResult.Built : PurchaseResult.CannotAfford;
        }
    }
}
