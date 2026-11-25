using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
using Stilargo.Core.Hill;
using Stilargo.Tests.EditMode.Combat;

namespace Stilargo.Tests.EditMode.Hill
{
    public sealed class HillControlTests
    {
        private const float Radius = 5f;
        private const float IncomePerSecond = 2f;
        private const float HoldSeconds = 10f;
        private const float UnitHealth = 10f;

        private static readonly Vector3 OnHill = new Vector3(1f, 0f, 1f);
        private static readonly Vector3 OffHill = new Vector3(50f, 0f, 50f);
        private static readonly IReadOnlyList<ICombatTarget> Nobody = Array.Empty<ICombatTarget>();

        private Wallet _wallet;
        private HillControl _hill;

        [SetUp]
        public void SetUp()
        {
            _wallet = new Wallet();
            HillSettings settings = new HillSettings(0f, 0f, Radius).WithIncome(IncomePerSecond).WithHoldDuration(HoldSeconds);
            _hill = new HillControl(new HillZone(Vector3.Zero, Radius), settings, _wallet);
        }

        [Test]
        public void Tick_NobodyOnHill_IsEmpty()
        {
            _hill.Tick(1f, Units(OffHill), Units(OffHill));

            Assert.AreEqual(HillState.Empty, _hill.State);
        }

        [Test]
        public void Tick_OnlyPlayer_HoldsAndEarns()
        {
            _hill.Tick(3f, Units(OnHill), Nobody);

            Assert.AreEqual(HillState.HeldByPlayer, _hill.State);
            Assert.AreEqual(6, _wallet.Balance);
            Assert.AreEqual(3f, _hill.HeldSeconds);
            Assert.AreEqual(HoldSeconds - 3f, _hill.RemainingHoldSeconds);
        }

        [Test]
        public void Tick_OnlyRival_HeldByRivalAndPaysNobody()
        {
            _hill.Tick(3f, Units(OffHill), Units(OnHill));

            Assert.AreEqual(HillState.HeldByRival, _hill.State);
            Assert.AreEqual(0, _wallet.Balance);
            Assert.AreEqual(0f, _hill.HeldSeconds);
        }

        [Test]
        public void Tick_BothOnHill_IsContestedAndNobodyEarns()
        {
            _hill.Tick(3f, Units(OnHill), Units(OnHill));

            Assert.AreEqual(HillState.Contested, _hill.State);
            Assert.AreEqual(0, _wallet.Balance);
            Assert.AreEqual(0f, _hill.HeldSeconds);
        }

        [Test]
        public void Tick_DeadUnitsDoNotCount()
        {
            var deadRival = new FakeCombatTarget(OnHill, UnitHealth);
            deadRival.TakeDamage(UnitHealth);

            _hill.Tick(1f, Units(OnHill), new ICombatTarget[] { deadRival });

            Assert.AreEqual(HillState.HeldByPlayer, _hill.State);
        }

        [Test]
        public void Tick_FractionalIncome_CarriesOver()
        {
            _hill.Tick(0.3f, Units(OnHill), Nobody);
            Assert.AreEqual(0, _wallet.Balance);

            _hill.Tick(0.3f, Units(OnHill), Nobody);
            Assert.AreEqual(1, _wallet.Balance);
        }

        [Test]
        public void Tick_HoldTimeAddsUpAcrossSeparateHolds()
        {
            _hill.Tick(4f, Units(OnHill), Nobody);
            _hill.Tick(4f, Units(OnHill), Units(OnHill));
            _hill.Tick(4f, Units(OnHill), Nobody);

            Assert.AreEqual(8f, _hill.HeldSeconds);
        }

        [Test]
        public void Tick_HoldDurationReached_RaisesHoldCompletedOnceAndCaps()
        {
            int completions = 0;
            _hill.HoldCompleted += () => completions++;

            _hill.Tick(HoldSeconds - 1f, Units(OnHill), Nobody);
            Assert.AreEqual(0, completions);

            _hill.Tick(5f, Units(OnHill), Nobody);
            _hill.Tick(5f, Units(OnHill), Nobody);

            Assert.AreEqual(1, completions);
            Assert.IsTrue(_hill.IsHoldComplete);
            Assert.AreEqual(HoldSeconds, _hill.HeldSeconds);
        }

        [Test]
        public void Tick_StateChanged_RaisedOnlyOnChange()
        {
            var states = new List<HillState>();
            _hill.StateChanged += states.Add;

            _hill.Tick(1f, Units(OnHill), Nobody);
            _hill.Tick(1f, Units(OnHill), Nobody);
            _hill.Tick(1f, Units(OnHill), Units(OnHill));

            CollectionAssert.AreEqual(new[] { HillState.HeldByPlayer, HillState.Contested }, states);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Tick_InvalidDelta_Throws(float delta)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _hill.Tick(delta, Nobody, Nobody));
        }

        [Test]
        public void Tick_NullLists_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => _hill.Tick(1f, null, Nobody));
            Assert.Throws<ArgumentNullException>(() => _hill.Tick(1f, Nobody, null));
        }

        private static IReadOnlyList<ICombatTarget> Units(Vector3 position)
        {
            return new ICombatTarget[] { new FakeCombatTarget(position, UnitHealth) };
        }
    }
}
