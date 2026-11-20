using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class UnitCombatTests
    {
        private const float Damage = 10f;
        private const float WeaponRange = 5f;
        private const float Cooldown = 1f;
        private const float SightRange = 15f;
        private const float TargetHealth = 100f;
        private const float DeltaSeconds = 0.1f;

        private static readonly Vector3 UnitPosition = Vector3.Zero;
        private static readonly Vector3 Anchor = new Vector3(-4f, 0f, -4f);

        private UnitCombat _combat;
        private FakeStance _stance;

        [SetUp]
        public void SetUp()
        {
            Weapon weapon = new Weapon(new WeaponSettings(Damage, WeaponRange, Cooldown));
            _combat = new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange));
            _stance = new FakeStance(StanceAction.Fire);
            _combat.SetStance(_stance);
        }

        private static CombatSituation Idle(params ICombatTarget[] enemies)
        {
            return new CombatSituation(new UnitCombatPose(UnitPosition, Anchor), false, enemies);
        }

        private static CombatSituation Moving(params ICombatTarget[] enemies)
        {
            return new CombatSituation(new UnitCombatPose(UnitPosition, Anchor), true, enemies);
        }

        private static FakeCombatTarget TargetAt(float x)
        {
            return new FakeCombatTarget(new Vector3(x, 0f, 0f), TargetHealth);
        }

        [Test]
        public void Constructor_RejectsWeaponRangeBeyondSight()
        {
            Weapon weapon = new Weapon(new WeaponSettings(Damage, SightRange + 1f, Cooldown));

            Assert.Throws<ArgumentException>(() => new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange)));
        }

        [Test]
        public void SetStance_RaisesStanceChanged()
        {
            IStance reported = null;
            FakeStance next = new FakeStance(StanceAction.Idle);
            _combat.StanceChanged += stance => reported = stance;

            _combat.SetStance(next);

            Assert.That(_combat.Stance, Is.SameAs(next));
            Assert.That(reported, Is.SameAs(next));
        }

        [Test]
        public void StanceFire_InRange_ShootsAndRaisesFired()
        {
            FakeCombatTarget target = TargetAt(3f);
            ICombatTarget firedAt = null;
            _combat.Fired += hit => firedAt = hit;

            CombatCommand command = _combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.Fire));
            Assert.That(firedAt, Is.SameAs(target));
            Assert.That(target.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
            Assert.That(_stance.LastContext.Target.IsInWeaponRange, Is.True);
        }

        [Test]
        public void FollowingOrders_TargetInRange_FiresButKeepsFollowing()
        {
            FakeCombatTarget target = TargetAt(3f);
            int firedCount = 0;
            _combat.Fired += _ => firedCount++;

            CombatCommand command = _combat.Tick(DeltaSeconds, Moving(target));

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.FollowOrders));
            Assert.That(firedCount, Is.EqualTo(1));
            Assert.That(_stance.DecideCount, Is.EqualTo(0));
        }

        [Test]
        public void FollowingOrders_TargetOutOfWeaponRange_DoesNotChaseOrFire()
        {
            _stance.Action = StanceAction.Chase;
            FakeCombatTarget target = TargetAt(10f);

            CombatCommand command = _combat.Tick(DeltaSeconds, Moving(target));

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.FollowOrders));
            Assert.That(target.CurrentHealth, Is.EqualTo(TargetHealth));
        }

        [Test]
        public void CandidatesBeyondSight_AreIgnored()
        {
            FakeCombatTarget target = TargetAt(SightRange + 1f);

            _combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(_stance.LastContext.Target.Exists, Is.False);
        }

        [Test]
        public void DeadCandidates_AreIgnored()
        {
            FakeCombatTarget target = TargetAt(3f);
            target.TakeDamage(TargetHealth);

            CombatCommand command = _combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(_stance.LastContext.Target.Exists, Is.False);
            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.Idle));
        }

        [Test]
        public void StanceChase_ReturnsTargetPositionAsDestination()
        {
            _stance.Action = StanceAction.Chase;
            FakeCombatTarget target = TargetAt(10f);

            CombatCommand command = _combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.MoveTo));
            Assert.That(command.Destination, Is.EqualTo(target.Position));
            Assert.That(_stance.LastContext.Target.IsInWeaponRange, Is.False);
        }

        [Test]
        public void StanceReturn_ReturnsAnchorAsDestination()
        {
            _stance.Action = StanceAction.ReturnToAnchor;

            CombatCommand command = _combat.Tick(DeltaSeconds, Idle());

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.MoveTo));
            Assert.That(command.Destination, Is.EqualTo(Anchor));
        }

        [Test]
        public void StanceIdle_DoesNotFire()
        {
            _stance.Action = StanceAction.Idle;
            FakeCombatTarget target = TargetAt(3f);

            CombatCommand command = _combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(command.Action, Is.EqualTo(CombatCommandAction.Idle));
            Assert.That(target.CurrentHealth, Is.EqualTo(TargetHealth));
        }

        [Test]
        public void Tick_WithoutStance_Throws()
        {
            Weapon weapon = new Weapon(new WeaponSettings(Damage, WeaponRange, Cooldown));
            UnitCombat combat = new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange));

            Assert.Throws<InvalidOperationException>(() => combat.Tick(DeltaSeconds, Idle()));
        }
    }
}
