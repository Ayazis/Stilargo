using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Combat.Stances;
using Stilargo.Core.Combat.Targeting;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class UnitCombatPositionTests
    {
        private const float Damage = 10f;
        private const float WeaponRange = 5f;
        private const float Cooldown = 1f;
        private const float SightRange = 15f;
        private const float TargetHealth = 100f;
        private const float DeltaSeconds = 0.1f;
        private const float HitChance = 0.5f;
        private const float MissRoll = 0.9f;
        private const float RangeBonus = 3f;
        private const float SightBonus = 5f;
        private const float CoverMultiplier = 0.5f;

        private static readonly Vector3 UnitPosition = Vector3.Zero;

        private FakeVisibilityQuery _visibility;
        private FakeStance _stance;

        [SetUp]
        public void SetUp()
        {
            _visibility = new FakeVisibilityQuery();
            _stance = new FakeStance(StanceAction.Fire);
        }

        private UnitCombat CreateCombat(IRandomSource random)
        {
            WeaponSettings settings = new WeaponSettings(Damage, WeaponRange, Cooldown).WithHitChance(HitChance);
            Weapon weapon = new Weapon(settings, random);
            UnitCombat combat = new UnitCombat(weapon, new NearestTargetPriority(), new CombatAwareness(SightRange, _visibility));
            combat.SetStance(_stance);
            return combat;
        }

        private UnitCombat CreateHittingCombat()
        {
            return CreateCombat(new FakeRandomSource(0f));
        }

        private static CombatSituation Idle(params ICombatTarget[] enemies)
        {
            return new CombatSituation(new UnitCombatPose(UnitPosition, UnitPosition), false, enemies);
        }

        private static FakeCombatTarget TargetAt(float x)
        {
            return new FakeCombatTarget(new Vector3(x, 0f, 0f), TargetHealth);
        }

        [Test]
        public void HiddenEnemyInRange_IsNotTargeted()
        {
            UnitCombat combat = CreateHittingCombat();
            FakeCombatTarget target = TargetAt(3f);
            _visibility.Hide(target.Position);

            combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(_stance.LastContext.Target.Exists, Is.False);
            Assert.That(target.CurrentHealth, Is.EqualTo(TargetHealth));
        }

        [Test]
        public void VisibleEnemy_IsTargetedWhileHiddenOneIsSkipped()
        {
            UnitCombat combat = CreateHittingCombat();
            FakeCombatTarget hidden = TargetAt(2f);
            FakeCombatTarget visible = TargetAt(4f);
            _visibility.Hide(hidden.Position);

            combat.Tick(DeltaSeconds, Idle(hidden, visible));

            Assert.That(_stance.LastContext.Target.Position, Is.EqualTo(visible.Position));
            Assert.That(visible.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
        }

        [Test]
        public void Miss_RaisesMissedNotFired()
        {
            UnitCombat combat = CreateCombat(new FakeRandomSource(MissRoll));
            FakeCombatTarget target = TargetAt(3f);
            ICombatTarget missed = null;
            int firedCount = 0;
            combat.Missed += shot => missed = shot;
            combat.Fired += _ => firedCount++;

            combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(missed, Is.SameAs(target));
            Assert.That(firedCount, Is.EqualTo(0));
        }

        [Test]
        public void NewUnit_HasNoPositionBonus()
        {
            UnitCombat combat = CreateHittingCombat();

            Assert.That(combat.Bonus.RangeMetres, Is.EqualTo(0f));
            Assert.That(combat.Protection.HitChanceMultiplier, Is.EqualTo(1f));
        }

        [Test]
        public void RangeBonus_LetsUnitShootBeyondBaseRange()
        {
            UnitCombat combat = CreateHittingCombat();
            FakeCombatTarget target = TargetAt(WeaponRange + 1f);
            combat.SetPositionBonus(new PositionBonus(RangeBonus, 0f, CombatProtection.None));

            combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(_stance.LastContext.Target.IsInWeaponRange, Is.True);
            Assert.That(target.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
        }

        [Test]
        public void SightBonus_RevealsEnemyBeyondBaseSight()
        {
            UnitCombat combat = CreateHittingCombat();
            FakeCombatTarget target = TargetAt(SightRange + 1f);
            combat.SetPositionBonus(new PositionBonus(0f, SightBonus, CombatProtection.None));

            combat.Tick(DeltaSeconds, Idle(target));

            Assert.That(_stance.LastContext.Target.Exists, Is.True);
        }

        [Test]
        public void SetPositionBonus_ExposesProtection()
        {
            UnitCombat combat = CreateHittingCombat();
            CombatProtection cover = new CombatProtection(CoverMultiplier, 1f);

            combat.SetPositionBonus(new PositionBonus(0f, 0f, cover));

            Assert.That(combat.Protection.HitChanceMultiplier, Is.EqualTo(CoverMultiplier));
        }

        [Test]
        public void Awareness_CanSee_RequiresSightAndVisibility()
        {
            CombatAwareness awareness = new CombatAwareness(SightRange, _visibility);
            Vector3 near = new Vector3(3f, 0f, 0f);
            Vector3 far = new Vector3(SightRange + 1f, 0f, 0f);

            Assert.That(awareness.CanSee(UnitPosition, near), Is.True);
            Assert.That(awareness.CanSee(UnitPosition, far), Is.False);

            _visibility.Hide(near);
            awareness.SetSightBonus(SightBonus);

            Assert.That(awareness.CanSee(UnitPosition, near), Is.False);
            Assert.That(awareness.CanSee(UnitPosition, far), Is.True);
            Assert.That(awareness.EffectiveSightRange, Is.EqualTo(SightRange + SightBonus));
        }

        [Test]
        public void Awareness_RejectsNegativeSightBonusAndNullVisibility()
        {
            CombatAwareness awareness = new CombatAwareness(SightRange);

            Assert.Throws<ArgumentOutOfRangeException>(() => awareness.SetSightBonus(-1f));
            Assert.Throws<ArgumentNullException>(() => new CombatAwareness(SightRange, null));
        }
    }
}
