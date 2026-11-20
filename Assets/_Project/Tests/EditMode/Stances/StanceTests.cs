using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat.Stances;

namespace Stilargo.Tests.EditMode.Stances
{
    public sealed class StanceTests
    {
        private const float LeashRadius = 10f;

        private static readonly Vector3 Anchor = Vector3.Zero;

        private static StanceContext AtAnchor(StanceTarget target)
        {
            return new StanceContext(Anchor, Anchor, target);
        }

        [Test]
        public void Names_AreAsShownToPlayer()
        {
            Assert.AreEqual("Aggressive", new AggressiveStance().Name);
            Assert.AreEqual("Defensive", new DefensiveStance(LeashRadius).Name);
            Assert.AreEqual("Hold", new HoldStance().Name);
        }

        [Test]
        public void Aggressive_TargetInRange_Fires()
        {
            StanceAction action = new AggressiveStance().Decide(AtAnchor(StanceTarget.At(new Vector3(5f, 0f, 0f), true)));

            Assert.AreEqual(StanceAction.Fire, action);
        }

        [Test]
        public void Aggressive_TargetOutOfRange_Chases()
        {
            StanceAction action = new AggressiveStance().Decide(AtAnchor(StanceTarget.At(new Vector3(50f, 0f, 0f), false)));

            Assert.AreEqual(StanceAction.Chase, action);
        }

        [Test]
        public void Aggressive_NoTarget_Idles()
        {
            StanceAction action = new AggressiveStance().Decide(new StanceContext(new Vector3(30f, 0f, 0f), Anchor, StanceTarget.None));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [Test]
        public void Hold_TargetInRange_Fires()
        {
            StanceAction action = new HoldStance().Decide(AtAnchor(StanceTarget.At(new Vector3(5f, 0f, 0f), true)));

            Assert.AreEqual(StanceAction.Fire, action);
        }

        [Test]
        public void Hold_TargetOutOfRange_Idles()
        {
            StanceAction action = new HoldStance().Decide(AtAnchor(StanceTarget.At(new Vector3(5f, 0f, 0f), false)));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [Test]
        public void Hold_NoTargetAwayFromAnchor_Idles()
        {
            StanceAction action = new HoldStance().Decide(new StanceContext(new Vector3(30f, 0f, 0f), Anchor, StanceTarget.None));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [Test]
        public void Defensive_TargetInRange_FiresEvenOutsideLeash()
        {
            StanceAction action = new DefensiveStance(LeashRadius).Decide(AtAnchor(StanceTarget.At(new Vector3(50f, 0f, 0f), true)));

            Assert.AreEqual(StanceAction.Fire, action);
        }

        [Test]
        public void Defensive_TargetOutOfRangeInsideLeash_Chases()
        {
            StanceAction action = new DefensiveStance(LeashRadius).Decide(AtAnchor(StanceTarget.At(new Vector3(9f, 0f, 0f), false)));

            Assert.AreEqual(StanceAction.Chase, action);
        }

        [Test]
        public void Defensive_TargetExactlyOnLeash_Chases()
        {
            StanceAction action = new DefensiveStance(LeashRadius).Decide(AtAnchor(StanceTarget.At(new Vector3(0f, 0f, LeashRadius), false)));

            Assert.AreEqual(StanceAction.Chase, action);
        }

        [Test]
        public void Defensive_LeashIgnoresHeight()
        {
            StanceAction action = new DefensiveStance(LeashRadius).Decide(AtAnchor(StanceTarget.At(new Vector3(9f, 100f, 0f), false)));

            Assert.AreEqual(StanceAction.Chase, action);
        }

        [Test]
        public void Defensive_TargetJustOutsideLeashAtAnchor_Idles()
        {
            StanceAction action = new DefensiveStance(LeashRadius).Decide(AtAnchor(StanceTarget.At(new Vector3(10.01f, 0f, 0f), false)));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [Test]
        public void Defensive_TargetOutsideLeashAwayFromAnchor_ReturnsToAnchor()
        {
            var context = new StanceContext(new Vector3(8f, 0f, 0f), Anchor, StanceTarget.At(new Vector3(20f, 0f, 0f), false));

            StanceAction action = new DefensiveStance(LeashRadius).Decide(context);

            Assert.AreEqual(StanceAction.ReturnToAnchor, action);
        }

        [Test]
        public void Defensive_NoTargetAwayFromAnchor_ReturnsToAnchor()
        {
            var unitPosition = new Vector3(DefensiveStance.AnchorArrivalTolerance + 0.01f, 0f, 0f);

            StanceAction action = new DefensiveStance(LeashRadius).Decide(new StanceContext(unitPosition, Anchor, StanceTarget.None));

            Assert.AreEqual(StanceAction.ReturnToAnchor, action);
        }

        [Test]
        public void Defensive_NoTargetWithinTolerance_Idles()
        {
            var unitPosition = new Vector3(DefensiveStance.AnchorArrivalTolerance, 0f, 0f);

            StanceAction action = new DefensiveStance(LeashRadius).Decide(new StanceContext(unitPosition, Anchor, StanceTarget.None));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [Test]
        public void Defensive_AnchorToleranceIgnoresHeight()
        {
            var unitPosition = new Vector3(0f, 5f, 0f);

            StanceAction action = new DefensiveStance(LeashRadius).Decide(new StanceContext(unitPosition, Anchor, StanceTarget.None));

            Assert.AreEqual(StanceAction.Idle, action);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Defensive_InvalidLeashRadius_Throws(float leashRadius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DefensiveStance(leashRadius));
        }

        [Test]
        public void Defensive_ExposesLeashRadius()
        {
            Assert.AreEqual(LeashRadius, new DefensiveStance(LeashRadius).LeashRadius);
        }
    }
}
