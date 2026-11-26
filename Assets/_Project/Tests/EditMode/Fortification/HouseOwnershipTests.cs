using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Scavenging;
using Stilargo.Tests.EditMode.Combat;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class HouseOwnershipTests
    {
        private HouseLedger _ledger;
        private HouseClaim _blue;
        private HouseClaim _red;
        private GarrisonBuilding _building;

        [SetUp]
        public void SetUp()
        {
            _ledger = FortificationTestHouse.CreateLedger();
            _blue = new HouseClaim(Team.Blue, _ledger);
            _red = new HouseClaim(Team.Red, _ledger);
            _building = FortificationTestHouse.CreateBuilding();
        }

        [Test]
        public void Claim_SetsOwner()
        {
            _blue.TryClaim(_building);

            Assert.IsTrue(_blue.House.IsOwnedBy(Team.Blue));
            Assert.IsFalse(_blue.House.IsUnoccupied);
        }

        [Test]
        public void OtherTeam_CannotClaimStandingHouse()
        {
            _blue.TryClaim(_building);

            Assert.IsFalse(_red.CanClaim(_building));
            Assert.IsFalse(_red.TryClaim(_building));
            Assert.IsTrue(_blue.House.IsOwnedBy(Team.Blue));
        }

        [Test]
        public void Destroyed_BecomesUnoccupiedAndLeavesOwner()
        {
            _blue.TryClaim(_building);
            ClaimedHouse house = _blue.House;
            ClaimedHouse lost = null;
            _blue.Lost += candidate => lost = candidate;

            house.TakeDamage(house.MaxHealth);

            Assert.IsTrue(house.IsUnoccupied);
            Assert.IsFalse(_blue.IsClaimed);
            Assert.AreSame(house, lost);
        }

        [Test]
        public void Destroyed_CanBeConfiscatedAndStaysDamaged()
        {
            _blue.TryClaim(_building);
            ClaimedHouse house = _blue.House;
            var wallet = new Wallet();
            wallet.Deposit(100000);
            new FortificationShop(wallet, FortificationTestHouse.Settings.Definitions).TryBuy(house, FortificationTestHouse.Settings.Definitions[0]);
            house.TakeDamage(house.MaxHealth);

            Assert.IsTrue(_red.TryClaim(_building));

            Assert.AreSame(house, _red.House);
            Assert.IsTrue(house.IsOwnedBy(Team.Red));
            Assert.IsFalse(house.IsAlive);
            Assert.Greater(house.FortifiedFraction, 0f);
        }

        [Test]
        public void ReclaimedRuin_CanBeTakenByTheOtherTeam()
        {
            _blue.TryClaim(_building);
            ClaimedHouse house = _blue.House;
            house.TakeDamage(house.MaxHealth);
            _blue.TryClaim(_building);
            ClaimedHouse lost = null;
            _blue.Lost += candidate => lost = candidate;

            Assert.IsTrue(_red.TryClaim(_building));

            Assert.AreSame(house, lost);
            Assert.IsFalse(_blue.IsClaimed);
        }

        [Test]
        public void Ledger_CreatesEachHouseOnce()
        {
            int created = 0;
            _ledger.HouseCreated += _ => created++;
            _blue.TryClaim(_building);
            _blue.House.TakeDamage(_blue.House.MaxHealth);

            _red.TryClaim(_building);

            Assert.AreEqual(1, created);
            Assert.AreEqual(1, _ledger.Houses.Count);
        }

        [Test]
        public void BaseLost_DropOffMovesToNextHouse()
        {
            DropOff dropOff = FortificationTestHouse.CreateDropOff();
            new BaseDropOff(_blue, dropOff);
            _blue.TryClaim(_building);
            GarrisonBuilding second = FortificationTestHouse.CreateBuilding();
            _blue.TryClaim(second);

            _blue.House.TakeDamage(_blue.House.MaxHealth);

            Assert.AreSame(second, _blue.House.Building);
            Assert.AreEqual(second.Entrance.X, dropOff.Position.X);
            Assert.AreEqual(second.Entrance.Z, dropOff.Position.Z);
        }

        [Test]
        public void HouseTarget_OnlyValidWhileOwnedByItsTeam()
        {
            _blue.TryClaim(_building);
            ClaimedHouse house = _blue.House;
            var blueTarget = new HouseTarget(house, Team.Blue);
            var redTarget = new HouseTarget(house, Team.Red);

            Assert.IsTrue(blueTarget.IsAlive);
            Assert.IsFalse(redTarget.IsAlive);

            redTarget.TakeDamage(house.MaxHealth);
            Assert.AreEqual(1f, house.HealthFraction, "A team's own house is not damaged through the other target.");

            blueTarget.TakeDamage(house.MaxHealth);
            Assert.IsFalse(blueTarget.IsAlive);
        }

        [Test]
        public void MountedTurret_MannedOnlyWhileItsTeamHoldsTheStandingHouse()
        {
            _blue.TryClaim(_building);
            ClaimedHouse house = _blue.House;
            var turret = new Turret(new Weapon(new WeaponSettings(1f, 10f, 1f)), house.Position, new FakeVisibilityQuery());
            var mounted = new MountedTurret(house, turret, Team.Blue);

            Assert.IsTrue(mounted.IsManned);

            house.TakeDamage(house.MaxHealth);
            Assert.IsFalse(mounted.IsManned);
        }
    }
}
