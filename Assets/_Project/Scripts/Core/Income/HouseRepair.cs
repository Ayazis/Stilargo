using System;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Income
{
    /// <summary>
    /// Restores a claimed house to full condition for money. The price grows with the house's size and how worn
    /// it is; battle damage is not repaired here.
    /// </summary>
    public sealed class HouseRepair
    {
        private readonly HouseIncomeSettings _settings;
        private readonly Wallet _wallet;

        public HouseRepair(HouseIncomeSettings settings, Wallet wallet)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        /// <summary>Whole money needed to restore the house; zero when it is already in full condition.</summary>
        public int CostFor(ClaimedHouse house)
        {
            if (house == null)
            {
                throw new ArgumentNullException(nameof(house));
            }

            float wornShare = ClaimedHouse.FullCondition - house.Condition;
            return (int)Math.Ceiling(wornShare * _settings.FullRepairCostMoneyPerFloorCell * house.FloorCells);
        }

        public bool CanRepair(ClaimedHouse house)
        {
            int cost = CostFor(house);
            return house.IsAlive && house.Condition < ClaimedHouse.FullCondition && (cost == 0 || _wallet.CanAfford(cost));
        }

        public bool TryRepair(ClaimedHouse house)
        {
            if (!CanRepair(house))
            {
                return false;
            }

            int cost = CostFor(house);
            if (cost > 0)
            {
                _wallet.TrySpend(cost);
            }

            house.RestoreCondition();
            return true;
        }
    }
}
