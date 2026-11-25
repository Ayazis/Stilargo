using System;
using NUnit.Framework;
using Stilargo.Core.Income;

namespace Stilargo.Tests.EditMode.Income
{
    public sealed class HouseIncomeSettingsTests
    {
        [Test]
        public void Default_UsesTheDefaultConstants()
        {
            HouseIncomeSettings settings = HouseIncomeSettings.Default;

            Assert.AreEqual(HouseIncomeSettings.DefaultBaseMoneyPerSecondPerHouse, settings.BaseMoneyPerSecondPerHouse);
            Assert.AreEqual(HouseIncomeSettings.DefaultMoneyPerSecondPerFloorCell, settings.MoneyPerSecondPerFloorCell);
            Assert.AreEqual(HouseIncomeSettings.DefaultFortifiedBonusFraction, settings.FortifiedBonusFraction);
            Assert.AreEqual(HouseIncomeSettings.DefaultOccupiedBonusFraction, settings.OccupiedBonusFraction);
            Assert.AreEqual(HouseIncomeSettings.DefaultMaintenanceBonusFraction, settings.MaintenanceBonusFraction);
            Assert.AreEqual(HouseIncomeSettings.DefaultFullWearSeconds, settings.FullWearSeconds);
            Assert.AreEqual(HouseIncomeSettings.DefaultFullRepairCostMoneyPerFloorCell, settings.FullRepairCostMoneyPerFloorCell);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidValues_Throw(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HouseIncomeSettings().WithEarnings(value, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HouseIncomeSettings().WithBonuses(0f, value, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new HouseIncomeSettings().WithUpkeep(1f, value));
        }

        [Test]
        public void WithUpkeep_ZeroWearSeconds_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HouseIncomeSettings().WithUpkeep(0f, 1f));
        }
    }
}
