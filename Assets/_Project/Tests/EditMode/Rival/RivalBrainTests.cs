using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Rival;

namespace Stilargo.Tests.EditMode.Rival
{
    public sealed class RivalBrainTests
    {
        private RivalTestWorld _test;
        private RivalBrain _brain;

        [SetUp]
        public void SetUp()
        {
            _test = new RivalTestWorld();
            _brain = new RivalBrain(_test.World, _test.Strategies);
        }

        [Test]
        public void Start_EntersTheInitialStrategy()
        {
            var changes = new List<IRivalStrategy>();
            _brain.StrategyChanged += changes.Add;

            _brain.Start();

            Assert.AreSame(_test.Strategies.Initial, _brain.Current);
            CollectionAssert.AreEqual(new[] { _test.Strategies.Initial }, changes);
            Assert.IsFalse(_test.Units[0].Orders.IsIdle);
        }

        [Test]
        public void Start_Twice_Throws()
        {
            _brain.Start();

            Assert.Throws<InvalidOperationException>(() => _brain.Start());
        }

        [Test]
        public void Tick_BeforeStart_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => _brain.Tick(0.1f));
        }

        [Test]
        public void Tick_InvalidDelta_Throws()
        {
            _brain.Start();

            Assert.Throws<ArgumentOutOfRangeException>(() => _brain.Tick(float.NaN));
        }

        [Test]
        public void Tick_SwitchesWhenTheStrategySaysSo()
        {
            _brain.Start();

            _brain.Tick(RivalTestWorld.ScavengeSeconds);

            Assert.AreSame(_test.Strategies.ContestHill, _brain.Current);
        }

        [Test]
        public void Tick_UnitsPickUpAndDeliverLootToTheRivalWallet()
        {
            _brain.Start();
            _test.ArriveAll();

            _brain.Tick(0.1f);
            Assert.IsTrue(_test.Cache.IsLooted);
            _test.ArriveAll();
            _brain.Tick(0.1f);

            Assert.AreEqual(RivalTestWorld.CacheValue, _test.RivalWallet.Balance);
        }

        [Test]
        public void Tick_WholeSquadDead_DoesNothing()
        {
            _brain.Start();
            foreach (FakeRivalUnit unit in _test.Units)
            {
                unit.IsAlive = false;
            }

            _brain.Tick(RivalTestWorld.ScavengeSeconds);

            Assert.AreSame(_test.Strategies.Scavenge, _brain.Current);
            Assert.IsTrue(_brain.IsDefeated);
        }

        [Test]
        public void IsDefeated_FalseWhileAnyUnitLives()
        {
            _test.Units[0].IsAlive = false;

            Assert.IsFalse(_brain.IsDefeated);
        }
    }
}
