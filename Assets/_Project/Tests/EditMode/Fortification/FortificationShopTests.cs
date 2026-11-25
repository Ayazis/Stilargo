using System;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class FortificationShopTests
    {
        private DropOff _dropOff;
        private HouseClaim _claim;
        private FortificationShop _shop;

        private Wallet Wallet => _dropOff.Wallet;

        [SetUp]
        public void SetUp()
        {
            _dropOff = FortificationTestHouse.CreateDropOff();
            _claim = FortificationTestHouse.CreateClaim(_dropOff);
            _shop = new FortificationShop(Wallet, FortificationTestHouse.Settings.Definitions);
        }

        [Test]
        public void TryBuy_WithoutClaimedHouse_IsRefused()
        {
            Wallet.Deposit(1000);

            Assert.AreEqual(PurchaseResult.NoClaimedHouse, _shop.TryBuy(_claim.House, Barricade));
            Assert.AreEqual(1000, Wallet.Balance);
        }

        [Test]
        public void TryBuy_Affordable_SpendsFillsSlotAndAddsHealth()
        {
            ClaimHouse();
            Wallet.Deposit(100);
            FortificationSlot built = null;
            _claim.House.FortificationBuilt += slot => built = slot;

            Assert.AreEqual(PurchaseResult.Built, _shop.TryBuy(_claim.House, Barricade));
            Assert.AreEqual(100 - Barricade.CostMoney, Wallet.Balance);
            Assert.AreSame(Barricade, built.Fortification);
            Assert.AreEqual(FortificationSlotKind.Entrance, built.Kind);
            Assert.AreEqual(FortificationSettings.DefaultHouseBaseHealthPoints + Barricade.HealthBonusPoints, _claim.House.MaxHealth);
            Assert.AreEqual(1f, _claim.House.HealthFraction);
        }

        [Test]
        public void TryBuy_TooExpensive_IsRefusedWithoutChanges()
        {
            ClaimHouse();
            Wallet.Deposit(Barricade.CostMoney - 1);

            Assert.AreEqual(PurchaseResult.CannotAfford, _shop.TryBuy(_claim.House, Barricade));
            Assert.AreEqual(Barricade.CostMoney - 1, Wallet.Balance);
            Assert.IsTrue(_claim.House.Slots.All(slot => !slot.IsFilled));
        }

        [Test]
        public void TryBuy_NoFreeSlotOfThatKind_IsRefusedWithoutSpending()
        {
            ClaimHouse();
            Wallet.Deposit(1000);
            _shop.TryBuy(_claim.House, Barricade);

            Assert.AreEqual(PurchaseResult.NoFreeSlot, _shop.TryBuy(_claim.House, Barricade));
            Assert.AreEqual(1000 - Barricade.CostMoney, Wallet.Balance);
        }

        [Test]
        public void TryBuy_FillsSlotsOfMatchingKindOneByOne()
        {
            ClaimHouse();
            Wallet.Deposit(10000);
            int roofSlots = _claim.House.Slots.Count(slot => slot.Kind == FortificationSlotKind.Roof);

            for (int index = 0; index < roofSlots; index++)
            {
                Assert.AreEqual(PurchaseResult.Built, _shop.TryBuy(_claim.House, Turret));
            }

            Assert.AreEqual(PurchaseResult.NoFreeSlot, _shop.TryBuy(_claim.House, Turret));
            Assert.IsTrue(_claim.House.Slots.Where(slot => slot.Kind == FortificationSlotKind.Roof).All(slot => slot.Fortification == Turret));
        }

        [Test]
        public void TryBuy_DestroyedHouse_IsRefused()
        {
            ClaimHouse();
            Wallet.Deposit(1000);
            _claim.House.TakeDamage(_claim.House.MaxHealth);

            Assert.AreEqual(PurchaseResult.HouseDestroyed, _shop.TryBuy(_claim.House, Barricade));
            Assert.AreEqual(1000, Wallet.Balance);
        }

        [Test]
        public void Check_ReportsWithoutBuying()
        {
            ClaimHouse();
            Wallet.Deposit(1000);

            Assert.AreEqual(PurchaseResult.Built, _shop.Check(_claim.House, Barricade));
            Assert.AreEqual(1000, Wallet.Balance);
            Assert.IsTrue(_claim.House.Slots.All(slot => !slot.IsFilled));
        }

        [Test]
        public void TryBuy_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _shop.TryBuy(_claim.House, null));
        }

        private static FortificationDefinition Barricade => FortificationTestHouse.Find("barricade");

        private static FortificationDefinition Turret => FortificationTestHouse.Find("turret");

        private void ClaimHouse()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
        }
    }
}
