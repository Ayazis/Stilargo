using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Hud;
using Stilargo.Core.Income;
using Stilargo.Core.Scavenging;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Hud
{
    public sealed class FortifyMenuTests
    {
        private const int PlentyOfMoney = 1000;

        private GarrisonBuilding _building;
        private GarrisonBuilding _otherBuilding;
        private Wallet _wallet;
        private HouseClaim _claim;
        private FortifyMenu _menu;

        [SetUp]
        public void SetUp()
        {
            DropOff dropOff = FortificationTestHouse.CreateDropOff();
            _wallet = dropOff.Wallet;
            _claim = FortificationTestHouse.CreateClaim(dropOff);
            _building = FortificationTestHouse.CreateBuilding();
            _otherBuilding = FortificationTestHouse.CreateBuilding();
            var target = new ClaimTarget(_claim, new ClaimCandidatePicker(new List<GarrisonBuilding> { _building, _otherBuilding }));
            var shop = new FortificationShop(_wallet, FortificationTestHouse.Settings.Definitions);
            HouseIncomeSettings incomeSettings = HouseIncomeSettings.Default;
            var upkeep = new UpkeepMenu(target, new HouseIncome(_claim, incomeSettings, _wallet), new HouseRepair(incomeSettings, _wallet));
            _menu = new FortifyMenu(target, shop, upkeep);
        }

        [Test]
        public void CanClaim_NobodyGarrisoned_IsFalse()
        {
            Assert.IsFalse(_menu.CanClaim);
            Assert.IsFalse(_menu.TryClaim());
            Assert.IsFalse(_menu.IsClaimed);
        }

        [Test]
        public void TryClaim_WithGarrisonedUnit_ClaimsThatBuilding()
        {
            object unit = new object();
            _building.TryOccupy(unit, out _);

            Assert.IsTrue(_menu.CanClaim);
            Assert.IsTrue(_menu.TryClaim());
            Assert.IsTrue(_menu.IsClaimed);
            Assert.AreSame(_building, _claim.House.Building);
            Assert.IsFalse(_menu.CanClaim);
        }

        [Test]
        public void TryClaim_SquadMovedToAnotherBuilding_ClaimsItToo()
        {
            object unit = new object();
            _building.TryOccupy(unit, out _);
            _menu.TryClaim();
            _building.Release(unit);
            _otherBuilding.TryOccupy(unit, out _);

            Assert.IsTrue(_menu.CanClaim);
            Assert.IsTrue(_menu.TryClaim());
            Assert.AreEqual(2, _claim.Houses.Count);
            Assert.AreSame(_building, _claim.House.Building);
        }

        [Test]
        public void Buy_SquadInSecondHouse_FortifiesThatHouse()
        {
            object unit = new object();
            _building.TryOccupy(unit, out _);
            _menu.TryClaim();
            _building.Release(unit);
            _otherBuilding.TryOccupy(unit, out _);
            _menu.TryClaim();
            _wallet.Deposit(PlentyOfMoney);

            _menu.Buy(0);

            Assert.AreEqual(0f, _claim.Houses[0].FortifiedFraction);
            Assert.Greater(_claim.Houses[1].FortifiedFraction, 0f);
            StringAssert.StartsWith(HudText.HouseName, _menu.HouseHealthText());
        }

        [Test]
        public void Offers_MatchTheShopCatalogue()
        {
            Assert.AreEqual(FortificationTestHouse.Settings.Definitions.Count, _menu.OfferCount);
            FortificationDefinition first = FortificationTestHouse.Settings.Definitions[0];
            Assert.AreEqual($"{first.DisplayName} (${first.CostMoney})", _menu.OfferLabel(0));
        }

        [Test]
        public void Buy_BeforeClaim_SaysClaimFirst()
        {
            _wallet.Deposit(PlentyOfMoney);

            Assert.AreEqual("Claim a house first.", _menu.Buy(0));
            Assert.IsFalse(_menu.IsOfferAvailable(0));
        }

        [Test]
        public void Buy_TooPoor_SaysSo()
        {
            ClaimHouse();

            Assert.AreEqual("Not enough money.", _menu.Buy(0));
        }

        [Test]
        public void Buy_Affordable_BuildsAndSpends()
        {
            ClaimHouse();
            _wallet.Deposit(PlentyOfMoney);
            int cost = FortificationTestHouse.Settings.Definitions[0].CostMoney;

            Assert.IsTrue(_menu.IsOfferAvailable(0));
            Assert.AreEqual("Built.", _menu.Buy(0));
            Assert.AreEqual(PlentyOfMoney - cost, _wallet.Balance);
        }

        [Test]
        public void HouseHealthText_ShowsBase()
        {
            ClaimHouse();

            Assert.AreEqual(HudText.HouseHealth(HudText.BaseName, _claim.House.CurrentHealth, _claim.House.MaxHealth), _menu.HouseHealthText());
        }

        [Test]
        public void HouseHealthText_BeforeClaim_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _menu.HouseHealthText());
        }

        [Test]
        public void Repair_WornHouse_RestoresConditionAndSpends()
        {
            ClaimHouse();
            _claim.House.Wear(0.5f);
            _wallet.Deposit(PlentyOfMoney);

            Assert.IsTrue(_menu.Upkeep.CanRepair());
            Assert.IsTrue(_menu.Upkeep.Repair());
            Assert.AreEqual(ClaimedHouse.FullCondition, _claim.House.Condition);
            Assert.Less(_wallet.Balance, PlentyOfMoney);
            Assert.AreEqual(UpkeepMenu.NothingToRepairLabel, _menu.Upkeep.RepairLabel());
        }

        [Test]
        public void Repair_BeforeClaim_IsRefused()
        {
            Assert.IsFalse(_menu.Upkeep.CanRepair());
            Assert.IsFalse(_menu.Upkeep.Repair());
        }

        [Test]
        public void OfferAccess_OutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _menu.OfferLabel(_menu.OfferCount));
            Assert.Throws<ArgumentOutOfRangeException>(() => _menu.Buy(-1));
        }

        private void ClaimHouse()
        {
            _claim.TryClaim(_building);
        }
    }
}
