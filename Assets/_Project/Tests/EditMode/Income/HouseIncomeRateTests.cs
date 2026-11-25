using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Income;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Income
{
    public sealed class HouseIncomeRateTests
    {
        private const float Tolerance = 0.0001f;
        private const float BasePerHouse = 1f;
        private const float PerFloorCell = 0.1f;
        private const float Bonus = 0.5f;

        private HouseClaim _claim;
        private ClaimedHouse _house;
        private HouseIncomeRate _rate;

        [SetUp]
        public void SetUp()
        {
            _claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
            _house = _claim.House;
            _rate = new HouseIncomeRate(new HouseIncomeSettings()
                .WithEarnings(BasePerHouse, PerFloorCell)
                .WithBonuses(Bonus, Bonus, Bonus));
        }

        private float SizeRate => BasePerHouse + (PerFloorCell * _house.FloorCells);

        [Test]
        public void FreshHouse_EarnsSizeRateWithFullMaintenanceBonus()
        {
            Assert.AreEqual(SizeRate * (1f + Bonus), _rate.MoneyPerSecond(_house), Tolerance);
        }

        [Test]
        public void WornHouse_EarnsLess()
        {
            float fresh = _rate.MoneyPerSecond(_house);

            _house.Wear(0.5f);

            Assert.AreEqual(SizeRate * (1f + (Bonus * 0.5f)), _rate.MoneyPerSecond(_house), Tolerance);
            Assert.Less(_rate.MoneyPerSecond(_house), fresh);
        }

        [Test]
        public void DamagedHouse_EarnsLess()
        {
            float fresh = _rate.MoneyPerSecond(_house);

            _house.TakeDamage(_house.MaxHealth / 2f);

            Assert.Less(_rate.MoneyPerSecond(_house), fresh);
        }

        [Test]
        public void OccupiedHouse_EarnsMore()
        {
            float empty = _rate.MoneyPerSecond(_house);

            _house.Building.TryOccupy(new object(), out _);

            Assert.AreEqual(empty + (SizeRate * Bonus * _house.OccupiedFraction), _rate.MoneyPerSecond(_house), Tolerance);
            Assert.Greater(_rate.MoneyPerSecond(_house), empty);
        }

        [Test]
        public void FortifiedHouse_EarnsMore()
        {
            float bare = _rate.MoneyPerSecond(_house);
            var wallet = new Wallet();
            wallet.Deposit(1000);

            new FortificationShop(wallet, FortificationTestHouse.Settings.Definitions).TryBuy(_house, FortificationTestHouse.Find("reinforcedWindow"));

            Assert.Greater(_house.FortifiedFraction, 0f);
            Assert.Greater(_rate.MoneyPerSecond(_house), bare);
        }

        [Test]
        public void BiggerHouse_EarnsMore()
        {
            Assert.Greater(_house.FloorCells, 0);
            var sizeless = new HouseIncomeRate(new HouseIncomeSettings().WithEarnings(BasePerHouse, 0f).WithBonuses(Bonus, Bonus, Bonus));

            Assert.Greater(_rate.MoneyPerSecond(_house), sizeless.MoneyPerSecond(_house));
        }

        [Test]
        public void DestroyedHouse_EarnsNothing()
        {
            _house.TakeDamage(_house.MaxHealth);

            Assert.AreEqual(0f, _rate.MoneyPerSecond(_house));
        }
    }
}
