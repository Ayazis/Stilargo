using System;

namespace Stilargo.Core.Income
{
    /// <summary>
    /// Tunable values for what claimed houses earn and how they wear, loaded from <c>Income.json</c>.
    /// Everything is validated on construction, so an invalid file fails at startup with a clear message.
    /// </summary>
    public sealed class HouseIncomeSettings
    {
        public const float DefaultBaseMoneyPerSecondPerHouse = 0.3f;
        public const float DefaultMoneyPerSecondPerFloorCell = 0.01f;
        public const float DefaultFortifiedBonusFraction = 0.5f;
        public const float DefaultOccupiedBonusFraction = 0.5f;
        public const float DefaultMaintenanceBonusFraction = 0.5f;
        public const float DefaultFullWearSeconds = 300f;
        public const float DefaultFullRepairCostMoneyPerFloorCell = 0.5f;

        public static HouseIncomeSettings Default => new HouseIncomeSettings();

        /// <summary>What every living claimed house earns regardless of size.</summary>
        public float BaseMoneyPerSecondPerHouse { get; private set; } = DefaultBaseMoneyPerSecondPerHouse;

        /// <summary>Extra earnings per floor cell (footprint cells times storeys).</summary>
        public float MoneyPerSecondPerFloorCell { get; private set; } = DefaultMoneyPerSecondPerFloorCell;

        /// <summary>Income bonus when every fortification slot is filled; scales with the filled share.</summary>
        public float FortifiedBonusFraction { get; private set; } = DefaultFortifiedBonusFraction;

        /// <summary>Income bonus when every garrison slot holds a unit; scales with the occupied share.</summary>
        public float OccupiedBonusFraction { get; private set; } = DefaultOccupiedBonusFraction;

        /// <summary>Income bonus for a house in full condition and full health; scales with maintenance.</summary>
        public float MaintenanceBonusFraction { get; private set; } = DefaultMaintenanceBonusFraction;

        /// <summary>Seconds for a house to wear from full condition to zero when nobody repairs it.</summary>
        public float FullWearSeconds { get; private set; } = DefaultFullWearSeconds;

        /// <summary>Cost of repairing a fully worn house, per floor cell; partial wear costs proportionally less.</summary>
        public float FullRepairCostMoneyPerFloorCell { get; private set; } = DefaultFullRepairCostMoneyPerFloorCell;

        public HouseIncomeSettings WithEarnings(float baseMoneyPerSecondPerHouse, float moneyPerSecondPerFloorCell)
        {
            BaseMoneyPerSecondPerHouse = RequireNonNegative(baseMoneyPerSecondPerHouse, nameof(baseMoneyPerSecondPerHouse));
            MoneyPerSecondPerFloorCell = RequireNonNegative(moneyPerSecondPerFloorCell, nameof(moneyPerSecondPerFloorCell));
            return this;
        }

        public HouseIncomeSettings WithBonuses(float fortifiedFraction, float occupiedFraction, float maintenanceFraction)
        {
            FortifiedBonusFraction = RequireNonNegative(fortifiedFraction, nameof(fortifiedFraction));
            OccupiedBonusFraction = RequireNonNegative(occupiedFraction, nameof(occupiedFraction));
            MaintenanceBonusFraction = RequireNonNegative(maintenanceFraction, nameof(maintenanceFraction));
            return this;
        }

        public HouseIncomeSettings WithUpkeep(float fullWearSeconds, float fullRepairCostMoneyPerFloorCell)
        {
            if (!(fullWearSeconds > 0f) || float.IsInfinity(fullWearSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(fullWearSeconds), fullWearSeconds, "Must be positive and finite.");
            }

            FullWearSeconds = fullWearSeconds;
            FullRepairCostMoneyPerFloorCell = RequireNonNegative(fullRepairCostMoneyPerFloorCell, nameof(fullRepairCostMoneyPerFloorCell));
            return this;
        }

        private static float RequireNonNegative(float value, string name)
        {
            if (!(value >= 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be zero or more and finite.");
            }

            return value;
        }
    }
}
