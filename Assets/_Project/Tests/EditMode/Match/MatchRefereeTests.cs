using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Match;
using Stilargo.Tests.EditMode.Combat;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Match
{
    public sealed class MatchRefereeTests
    {
        private const float HoldSeconds = 5f;
        private const float UnitHealth = 1f;

        private static readonly IReadOnlyList<ICombatTarget> Nobody = Array.Empty<ICombatTarget>();

        private HillControl _hill;
        private HouseClaim _claim;
        private MatchReferee _referee;
        private List<MatchOutcome> _decisions;

        [SetUp]
        public void SetUp()
        {
            HillSettings settings = new HillSettings(0f, 0f, 1f).WithHoldDuration(HoldSeconds);
            _hill = new HillControl(new HillZone(Vector3.Zero, 1f), settings);
            _claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            _referee = new MatchReferee(_hill, _claim);
            _decisions = new List<MatchOutcome>();
            _referee.Decided += _decisions.Add;
        }

        [Test]
        public void StartsUndecided()
        {
            Assert.AreEqual(MatchOutcome.Undecided, _referee.Outcome);
            Assert.IsFalse(_referee.IsDecided);
        }

        [Test]
        public void HoldingTheHillLongEnough_Wins()
        {
            HoldHill();

            Assert.AreEqual(MatchOutcome.Won, _referee.Outcome);
            CollectionAssert.AreEqual(new[] { MatchOutcome.Won }, _decisions);
        }

        [Test]
        public void NoHouseButAUnitAlive_DoesNotLose()
        {
            _referee.Review(new ICombatTarget[] { AliveUnit() });

            Assert.IsFalse(_referee.IsDecided);
        }

        [Test]
        public void HouseButNoUnitsAlive_DoesNotLose()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());

            _referee.Review(new ICombatTarget[] { DeadUnit() });

            Assert.IsFalse(_referee.IsDecided);
        }

        [Test]
        public void NoHouseAndNoUnitsAlive_Loses()
        {
            _referee.Review(new ICombatTarget[] { DeadUnit() });

            Assert.AreEqual(MatchOutcome.Lost, _referee.Outcome);
            CollectionAssert.AreEqual(new[] { MatchOutcome.Lost }, _decisions);
        }

        [Test]
        public void LastHouseDestroyedAndNoUnits_Loses()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
            ClaimedHouse house = _claim.House;

            house.TakeDamage(house.MaxHealth);
            _referee.Review(Nobody);

            Assert.AreEqual(MatchOutcome.Lost, _referee.Outcome);
        }

        [Test]
        public void BaseDestroyedWithUnitsAlive_DoesNotLose()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
            ClaimedHouse house = _claim.House;

            house.TakeDamage(house.MaxHealth);
            _referee.Review(new ICombatTarget[] { AliveUnit() });

            Assert.IsFalse(_referee.IsDecided);
        }

        [Test]
        public void FirstResultIsFinal()
        {
            HoldHill();

            _referee.Review(Nobody);

            Assert.AreEqual(MatchOutcome.Won, _referee.Outcome);
            Assert.AreEqual(1, _decisions.Count);
        }

        [Test]
        public void InvalidArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new MatchReferee(null, _claim));
            Assert.Throws<ArgumentNullException>(() => new MatchReferee(_hill, null));
            Assert.Throws<ArgumentNullException>(() => _referee.Review(null));
        }

        private static ICombatTarget AliveUnit()
        {
            return new FakeCombatTarget(Vector3.Zero, UnitHealth);
        }

        private static ICombatTarget DeadUnit()
        {
            var unit = new FakeCombatTarget(Vector3.Zero, UnitHealth);
            unit.TakeDamage(UnitHealth);
            return unit;
        }

        private void HoldHill()
        {
            _hill.Tick(HoldSeconds, new ICombatTarget[] { AliveUnit() }, Nobody);
        }
    }
}
