using System;
using Stilargo.Core.Income;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Income.json</c>. Defaults match <see cref="HouseIncomeSettings.Default"/>.</summary>
    [Serializable]
    public sealed class IncomeConfig
    {
        public const string FileName = "Income.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float baseMoneyPerSecondPerHouse = HouseIncomeSettings.DefaultBaseMoneyPerSecondPerHouse;
        [SerializeField] private float moneyPerSecondPerFloorCell = HouseIncomeSettings.DefaultMoneyPerSecondPerFloorCell;
        [SerializeField] private float fortifiedBonusFraction = HouseIncomeSettings.DefaultFortifiedBonusFraction;
        [SerializeField] private float occupiedBonusFraction = HouseIncomeSettings.DefaultOccupiedBonusFraction;
        [SerializeField] private float maintenanceBonusFraction = HouseIncomeSettings.DefaultMaintenanceBonusFraction;
        [SerializeField] private float fullWearSeconds = HouseIncomeSettings.DefaultFullWearSeconds;
        [SerializeField] private float fullRepairCostMoneyPerFloorCell = HouseIncomeSettings.DefaultFullRepairCostMoneyPerFloorCell;

        public static HouseIncomeSettings Load()
        {
            return ConfigFile.Read<IncomeConfig>(FileName).ToSettings();
        }

        public HouseIncomeSettings ToSettings()
        {
            try
            {
                return new HouseIncomeSettings()
                    .WithEarnings(baseMoneyPerSecondPerHouse, moneyPerSecondPerFloorCell)
                    .WithBonuses(fortifiedBonusFraction, occupiedBonusFraction, maintenanceBonusFraction)
                    .WithUpkeep(fullWearSeconds, fullRepairCostMoneyPerFloorCell);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
