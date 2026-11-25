using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
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

        private HillControl _hill;
        private HouseClaim _claim;
        private MatchReferee _referee;
        private List<MatchOutcome> _decisions;

        [SetUp]
        public void SetUp()
        {
            HillSettings settings = new HillSettings(0f, 0f, 1f).WithHoldDuration(HoldSeconds);
            _hill = new HillControl(new HillZone(Vector3.Zero, 1f), settings, new Wallet());
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
        public void HouseDestroyed_Loses()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());

            DestroyHouse();

            Assert.AreEqual(MatchOutcome.Lost, _referee.Outcome);
            CollectionAssert.AreEqual(new[] { MatchOutcome.Lost }, _decisions);
        }

        [Test]
        public void HouseClaimedBeforeTheReferee_StillLosesWhenDestroyed()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            claim.TryClaim(FortificationTestHouse.CreateBuilding());
            var referee = new MatchReferee(_hill, claim);

            claim.House.TakeDamage(claim.House.MaxHealth);

            Assert.AreEqual(MatchOutcome.Lost, referee.Outcome);
        }

        [Test]
        public void FirstResultIsFinal()
        {
            _claim.TryClaim(FortificationTestHouse.CreateBuilding());
            HoldHill();

            DestroyHouse();

            Assert.AreEqual(MatchOutcome.Won, _referee.Outcome);
            Assert.AreEqual(1, _decisions.Count);
        }

        [Test]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new MatchReferee(null, _claim));
            Assert.Throws<ArgumentNullException>(() => new MatchReferee(_hill, null));
        }

        private void HoldHill()
        {
            var player = new ICombatTarget[] { new FakeCombatTarget(Vector3.Zero, 1f) };
            _hill.Tick(HoldSeconds, player, Array.Empty<ICombatTarget>());
        }

        private void DestroyHouse()
        {
            _claim.House.TakeDamage(_claim.House.MaxHealth);
        }
    }
}
