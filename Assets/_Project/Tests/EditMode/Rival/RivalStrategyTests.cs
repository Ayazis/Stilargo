using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Income;
using Stilargo.Core.Rival;

namespace Stilargo.Tests.EditMode.Rival
{
    public sealed class RivalStrategyTests
    {
        private RivalTestWorld _test;

        [SetUp]
        public void SetUp()
        {
            _test = new RivalTestWorld();
        }

        [Test]
        public void Scavenge_Enter_SendsUnitsToTheNearestCache()
        {
            _test.Strategies.Scavenge.Enter(_test.World);

            foreach (FakeRivalUnit unit in _test.Units)
            {
                Assert.AreEqual(RivalTestWorld.CachePosition, unit.FakeOrders.LastTarget);
            }
        }

        [Test]
        public void Scavenge_CarryingUnit_GoesHome()
        {
            FakeRivalUnit unit = _test.Units[0];
            unit.Position = RivalTestWorld.CachePosition;
            unit.Carrier.PickUp(_test.Cache);

            _test.Strategies.Scavenge.Enter(_test.World);

            Assert.AreEqual(RivalTestWorld.Base, unit.FakeOrders.LastTarget);
        }

        [Test]
        public void Scavenge_NothingToFetch_LeavesUnitsWhereTheyAre()
        {
            _test.Units[0].Position = RivalTestWorld.HillCenter;
            _test.Cache.Take();

            _test.Strategies.Scavenge.Enter(_test.World);

            CollectionAssert.IsEmpty(_test.Units[0].FakeOrders.MoveTargets);
        }

        [Test]
        public void Scavenge_IdleUnitThatPickedUpLoot_IsSentHome()
        {
            IRivalStrategy scavenge = _test.Strategies.Scavenge;
            scavenge.Enter(_test.World);
            _test.ArriveAll();
            _test.Units[0].Carrier.PickUp(_test.Cache);

            Assert.AreSame(scavenge, scavenge.Tick(_test.World, _test.Strategies, 0.1f));
            Assert.AreEqual(RivalTestWorld.Base, _test.Units[0].FakeOrders.LastTarget);
        }

        [Test]
        public void Scavenge_NothingLeftToFetch_ContestsTheHill()
        {
            IRivalStrategy scavenge = _test.Strategies.Scavenge;
            scavenge.Enter(_test.World);
            _test.Cache.Take();

            Assert.AreSame(_test.Strategies.ContestHill, scavenge.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Scavenge_CacheBeyondTheScavengeRadius_IsIgnored()
        {
            _test.Settings.WithScavengeRadius(RivalTestWorld.CachePosition.X - 1f);

            _test.Strategies.Scavenge.Enter(_test.World);

            CollectionAssert.IsEmpty(_test.Units[0].FakeOrders.MoveTargets);
            Assert.AreSame(_test.Strategies.ContestHill, _test.Strategies.Scavenge.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Scavenge_TimeUp_ContestsTheHill()
        {
            IRivalStrategy scavenge = _test.Strategies.Scavenge;
            scavenge.Enter(_test.World);

            Assert.AreSame(scavenge, scavenge.Tick(_test.World, _test.Strategies, RivalTestWorld.ScavengeSeconds - 1f));
            Assert.AreSame(_test.Strategies.ContestHill, scavenge.Tick(_test.World, _test.Strategies, 1f));
        }

        [Test]
        public void Scavenge_PlayerHoldsHill_ContestsTheHillAtOnce()
        {
            IRivalStrategy scavenge = _test.Strategies.Scavenge;
            scavenge.Enter(_test.World);
            _test.PutPlayerOnHill();

            Assert.AreSame(_test.Strategies.ContestHill, scavenge.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Contest_Enter_SpreadsUnitsAroundTheHill()
        {
            _test.Strategies.ContestHill.Enter(_test.World);

            Vector3 first = _test.Units[0].FakeOrders.LastTarget;
            Vector3 second = _test.Units[1].FakeOrders.LastTarget;
            Assert.AreNotEqual(first, second);
            Assert.IsTrue(_test.Hill.Zone.Contains(first));
            Assert.IsTrue(_test.Hill.Zone.Contains(second));
        }

        [Test]
        public void Contest_IdleUnitOffTheHill_IsSentBack()
        {
            IRivalStrategy contest = _test.Strategies.ContestHill;
            contest.Enter(_test.World);
            _test.ArriveAll();
            FakeRivalUnit straggler = _test.Units[0];
            straggler.Position = RivalTestWorld.Base;
            int ordersBefore = straggler.FakeOrders.MoveTargets.Count;

            contest.Tick(_test.World, _test.Strategies, 0.1f);

            Assert.AreEqual(ordersBefore + 1, straggler.FakeOrders.MoveTargets.Count);
            Assert.AreEqual(ordersBefore, _test.Units[1].FakeOrders.MoveTargets.Count);
        }

        [Test]
        public void Contest_TimeUpWithoutHouse_Scavenges()
        {
            IRivalStrategy contest = _test.Strategies.ContestHill;
            contest.Enter(_test.World);

            Assert.AreSame(_test.Strategies.Scavenge, contest.Tick(_test.World, _test.Strategies, RivalTestWorld.ContestSeconds));
        }

        [Test]
        public void Contest_TimeUpWithStandingHouse_Raids()
        {
            _test.ClaimHouse();
            IRivalStrategy contest = _test.Strategies.ContestHill;
            contest.Enter(_test.World);

            Assert.AreSame(_test.Strategies.RaidHouse, contest.Tick(_test.World, _test.Strategies, RivalTestWorld.ContestSeconds));
        }

        [Test]
        public void Contest_TimeUpWhilePlayerHoldsHill_KeepsContesting()
        {
            IRivalStrategy contest = _test.Strategies.ContestHill;
            contest.Enter(_test.World);
            _test.PutPlayerOnHill();

            Assert.AreSame(contest, contest.Tick(_test.World, _test.Strategies, RivalTestWorld.ContestSeconds));
        }

        [Test]
        public void Raid_Enter_SendsUnitsToTheEntrance()
        {
            _test.ClaimHouse();

            _test.Strategies.RaidHouse.Enter(_test.World);

            Assert.AreEqual(_test.Claim.House.Position, _test.Units[0].FakeOrders.LastTarget);
            Assert.AreEqual(_test.Claim.House.Position, _test.Units[1].FakeOrders.LastTarget);
        }

        [Test]
        public void Raid_HouseDestroyed_ConfiscatesIt()
        {
            ClaimedHouse house = _test.ClaimHouse();
            IRivalStrategy raid = _test.Strategies.RaidHouse;
            raid.Enter(_test.World);
            house.TakeDamage(house.MaxHealth);

            Assert.AreSame(_test.Strategies.ConfiscateHouse, raid.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Raid_HouseTakenByPlayerAgain_StaysOnIt()
        {
            ClaimedHouse house = _test.ClaimHouse();
            IRivalStrategy raid = _test.Strategies.RaidHouse;
            raid.Enter(_test.World);

            Assert.AreSame(raid, raid.Tick(_test.World, _test.Strategies, 0.1f));
            Assert.AreEqual(house.Position, _test.Units[0].FakeOrders.LastTarget);
        }

        [Test]
        public void Contest_TimeUpWithOnlyARuin_Confiscates()
        {
            _test.CreateRuin();
            IRivalStrategy contest = _test.Strategies.ContestHill;
            contest.Enter(_test.World);

            Assert.AreSame(_test.Strategies.ConfiscateHouse, contest.Tick(_test.World, _test.Strategies, RivalTestWorld.ContestSeconds));
        }

        [Test]
        public void Confiscate_Enter_SendsUnitsToTheRuin()
        {
            ClaimedHouse ruin = _test.CreateRuin();

            _test.Strategies.ConfiscateHouse.Enter(_test.World);

            Assert.AreEqual(ruin.Position, _test.Units[0].FakeOrders.LastTarget);
            Assert.AreEqual(ruin.Position, _test.Units[1].FakeOrders.LastTarget);
        }

        [Test]
        public void Confiscate_UnitReachesEntrance_ClaimsForTheRivalAndReturnsToTheHill()
        {
            ClaimedHouse ruin = _test.CreateRuin();
            IRivalStrategy confiscate = _test.Strategies.ConfiscateHouse;
            confiscate.Enter(_test.World);
            _test.Units[0].Position = ruin.Position;

            Assert.AreSame(_test.Strategies.ContestHill, confiscate.Tick(_test.World, _test.Strategies, 0.1f));
            Assert.IsTrue(ruin.IsOwnedBy(Team.Red));
            Assert.AreSame(ruin, _test.RivalClaim.House);
            Assert.IsFalse(_test.Claim.IsClaimed);
        }

        [Test]
        public void Confiscate_PlayerReclaimedRuin_IsStillConfiscatable()
        {
            ClaimedHouse ruin = _test.CreateRuin();
            IRivalStrategy confiscate = _test.Strategies.ConfiscateHouse;
            confiscate.Enter(_test.World);
            _test.Claim.TryClaim(ruin.Building);

            Assert.AreSame(confiscate, confiscate.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Confiscate_PlayerRepairedItFirst_ReturnsToTheHill()
        {
            ClaimedHouse ruin = _test.CreateRuin();
            IRivalStrategy confiscate = _test.Strategies.ConfiscateHouse;
            confiscate.Enter(_test.World);
            _test.Claim.TryClaim(ruin.Building);
            var wallet = new Wallet();
            wallet.Deposit(100000);
            new HouseRepair(new HouseIncomeSettings(), wallet).TryRepair(ruin);

            Assert.AreSame(_test.Strategies.ContestHill, confiscate.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Confiscate_TimeUp_ReturnsToTheHill()
        {
            _test.CreateRuin();
            IRivalStrategy confiscate = _test.Strategies.ConfiscateHouse;
            confiscate.Enter(_test.World);

            Assert.AreSame(_test.Strategies.ContestHill, confiscate.Tick(_test.World, _test.Strategies, RivalTestWorld.ConfiscateSeconds));
        }

        [Test]
        public void Raid_PlayerHoldsHill_ReturnsToTheHill()
        {
            _test.ClaimHouse();
            IRivalStrategy raid = _test.Strategies.RaidHouse;
            raid.Enter(_test.World);
            _test.PutPlayerOnHill();

            Assert.AreSame(_test.Strategies.ContestHill, raid.Tick(_test.World, _test.Strategies, 0.1f));
        }

        [Test]
        public void Raid_TimeUp_ReturnsToTheHill()
        {
            _test.ClaimHouse();
            IRivalStrategy raid = _test.Strategies.RaidHouse;
            raid.Enter(_test.World);

            Assert.AreSame(raid, raid.Tick(_test.World, _test.Strategies, RivalTestWorld.RaidSeconds - 1f));
            Assert.AreSame(_test.Strategies.ContestHill, raid.Tick(_test.World, _test.Strategies, 1f));
        }

        [Test]
        public void DeadUnits_GetNoOrders()
        {
            _test.Units[0].IsAlive = false;

            _test.Strategies.ContestHill.Enter(_test.World);

            CollectionAssert.IsEmpty(_test.Units[0].FakeOrders.MoveTargets);
            Assert.AreEqual(1, _test.Units[1].FakeOrders.MoveTargets.Count);
        }

        [Test]
        public void SendUnlessNear_WithinTolerance_GivesNoOrder()
        {
            var unit = new FakeRivalUnit(Vector3.Zero);

            RivalSquad.SendUnlessNear(unit, new Vector3(0.5f, 9f, 0f), 1f);

            CollectionAssert.IsEmpty(unit.FakeOrders.MoveTargets);
        }

        [Test]
        public void SendUnlessNear_BeyondTolerance_Orders()
        {
            var unit = new FakeRivalUnit(Vector3.Zero);

            RivalSquad.SendUnlessNear(unit, new Vector3(3f, 0f, 0f), 1f);

            Assert.AreEqual(new Vector3(3f, 0f, 0f), unit.FakeOrders.LastTarget);
        }
    }
}
