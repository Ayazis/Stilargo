using System;
using Stilargo.Core.Fortification;
using Stilargo.Core.Income;

namespace Stilargo.Core.Hud
{
    /// <summary>What the HUD shows about income and upkeep, and the repair action for the selected house.</summary>
    public sealed class UpkeepMenu
    {
        public const string NothingToRepairLabel = "Repair (in full condition)";

        private readonly ClaimTarget _target;
        private readonly HouseIncome _income;
        private readonly HouseRepair _repair;

        public UpkeepMenu(ClaimTarget target, HouseIncome income, HouseRepair repair)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _income = income ?? throw new ArgumentNullException(nameof(income));
            _repair = repair ?? throw new ArgumentNullException(nameof(repair));
        }

        public string IncomeText()
        {
            return HudText.Income(_target.ClaimedCount, _income.TotalMoneyPerSecond);
        }

        public string HouseText()
        {
            ClaimedHouse house = RequireSelectedHouse();
            return HudText.HouseUpkeep(house.Condition, _income.Rate.MoneyPerSecond(house));
        }

        public string RepairLabel()
        {
            ClaimedHouse house = RequireSelectedHouse();
            return house.NeedsRepair ? $"Repair (${_repair.CostFor(house)})" : NothingToRepairLabel;
        }

        public bool CanRepair()
        {
            ClaimedHouse house = _target.SelectedHouse;
            return house != null && _repair.CanRepair(house);
        }

        /// <summary>Repairs the selected house; false when it cannot be repaired now.</summary>
        public bool Repair()
        {
            ClaimedHouse house = _target.SelectedHouse;
            return house != null && _repair.TryRepair(house);
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
