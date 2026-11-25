using System;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Income
{
    /// <summary>
    /// What one claimed house earns per second. Size sets the base rate; barricading, occupation and maintenance
    /// each add a bonus on top, so a well-kept, manned fortress earns the most. A destroyed house earns nothing.
    /// </summary>
    public sealed class HouseIncomeRate
    {
        private readonly HouseIncomeSettings _settings;

        public HouseIncomeRate(HouseIncomeSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public float MoneyPerSecond(ClaimedHouse house)
        {
            if (house == null)
            {
                throw new ArgumentNullException(nameof(house));
            }

            if (!house.IsAlive)
            {
                return 0f;
            }

            return SizeRate(house) * (1f + Bonus(house));
        }

        private float SizeRate(ClaimedHouse house)
        {
            return _settings.BaseMoneyPerSecondPerHouse + (_settings.MoneyPerSecondPerFloorCell * house.FloorCells);
        }

        private float Bonus(ClaimedHouse house)
        {
            return (_settings.FortifiedBonusFraction * house.FortifiedFraction)
                + (_settings.OccupiedBonusFraction * house.OccupiedFraction)
                + (_settings.MaintenanceBonusFraction * house.Maintenance);
        }
    }
}
