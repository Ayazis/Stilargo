using System;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Income;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Income
{
    public sealed class HouseRepairTests
    {
        private const float CostPerFloorCell = 1f;

        private Wallet _wallet;
        private ClaimedHouse _house;
        private HouseRepair _repair;

        [SetUp]
        public void SetUp()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            claim.TryClaim(FortificationTestHouse.CreateBuilding());
            _house = claim.House;
            _wallet = new Wallet();
            _repair = new HouseRepair(new HouseIncomeSettings().WithUpkeep(100f, CostPerFloorCell), _wallet);
        }

        [Test]
        public void CostFor_FullCondition_IsZeroAndCannotRepair()
        {
            Assert.AreEqual(0, _repair.CostFor(_house));
            Assert.IsFalse(_repair.CanRepair(_house));
        }

        [Test]
        public void CostFor_ScalesWithWearAndSize()
        {
            _house.Wear(0.5f);

            Assert.AreEqual((int)Math.Ceiling(0.5f * CostPerFloorCell * _house.FloorCells), _repair.CostFor(_house));
        }

        [Test]
        public void TryRepair_Affordable_RestoresAndSpends()
        {
            _house.Wear(0.5f);
            int cost = _repair.CostFor(_house);
            _wallet.Deposit(cost + 5);

            Assert.IsTrue(_repair.TryRepair(_house));
            Assert.AreEqual(ClaimedHouse.FullCondition, _house.Condition);
            Assert.AreEqual(5, _wallet.Balance);
        }

        [Test]
        public void TryRepair_TooPoor_IsRefused()
        {
            _house.Wear(0.5f);

            Assert.IsFalse(_repair.TryRepair(_house));
            Assert.AreEqual(0.5f, _house.Condition);
        }

        [Test]
        public void TryRepair_DestroyedHouse_IsRefused()
        {
            _house.Wear(0.5f);
            _wallet.Deposit(1000);
            _house.TakeDamage(_house.MaxHealth);

            Assert.IsFalse(_repair.TryRepair(_house));
        }

        [Test]
        public void Wear_NeverBelowZero()
        {
            _house.Wear(5f);

            Assert.AreEqual(0f, _house.Condition);
            Assert.Throws<ArgumentOutOfRangeException>(() => _house.Wear(-1f));
        }
    }
}
