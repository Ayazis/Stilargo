using System;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Income;
using Stilargo.Core.Scavenging;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Income
{
    public sealed class HouseIncomeTests
    {
        private const float Tolerance = 0.0001f;
        private const float WearSeconds = 100f;

        private Wallet _wallet;
        private HouseClaim _claim;
        private HouseIncome _income;

        [SetUp]
        public void SetUp()
        {
            DropOff dropOff = FortificationTestHouse.CreateDropOff();
            _wallet = dropOff.Wallet;
            _claim = FortificationTestHouse.CreateClaim(dropOff);
            HouseIncomeSettings settings = new HouseIncomeSettings()
                .WithEarnings(1f, 0f)
                .WithBonuses(0f, 0f, 0f)
                .WithUpkeep(WearSeconds, 1f);
            _income = new HouseIncome(_claim, settings, _wallet);
        }

        [Test]
        public void Tick_NothingClaimed_PaysNothing()
        {
            _income.Tick(10f);

            Assert.AreEqual(0, _wallet.Balance);
        }

        [Test]
        public void Tick_PaysEveryClaimedHouse()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());

            _income.Tick(3f);

            Assert.AreEqual(2f, _income.TotalMoneyPerSecond, Tolerance);
            Assert.AreEqual(6, _wallet.Balance);
        }

        [Test]
        public void Tick_FractionalIncome_CarriesOver()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());

            _income.Tick(0.6f);
            Assert.AreEqual(0, _wallet.Balance);

            _income.Tick(0.6f);
            Assert.AreEqual(1, _wallet.Balance);
        }

        [Test]
        public void Tick_WearsLivingHouses()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());

            _income.Tick(WearSeconds / 4f);

            Assert.AreEqual(0.75f, _claim.House.Condition, Tolerance);
        }

        [Test]
        public void Tick_NegativeDelta_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _income.Tick(-1f));
        }
    }
}
