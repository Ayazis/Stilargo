using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class WeaponHitChanceTests
    {
        private const float Damage = 10f;
        private const float Range = 5f;
        private const float Cooldown = 2f;
        private const float TargetHealth = 100f;
        private const float HitChance = 0.5f;
        private const float LowRoll = 0.3f;
        private const float HighRoll = 0.7f;
        private const float AlmostOne = 0.9999f;
        private const float HalfCover = 0.5f;
        private const float GarrisonDamage = 0.25f;
        private const float RangeBonus = 3f;

        private FakeCombatTarget _target;

        [SetUp]
        public void SetUp()
        {
            _target = new FakeCombatTarget(new Vector3(3f, 0f, 0f), TargetHealth);
        }

        private static Weapon WeaponRolling(float roll)
        {
            WeaponSettings settings = new WeaponSettings(Damage, Range, Cooldown).WithHitChance(HitChance);
            return new Weapon(settings, new FakeRandomSource(roll));
        }

        [Test]
        public void DefaultHitChance_IsOneAndAlwaysHits()
        {
            WeaponSettings settings = new WeaponSettings(Damage, Range, Cooldown);
            Weapon weapon = new Weapon(settings, new FakeRandomSource(AlmostOne));

            Assert.That(settings.HitChance, Is.EqualTo(1f));
            Assert.That(weapon.TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.Hit));
        }

        [Test]
        public void RollBelowHitChance_Hits()
        {
            Assert.That(WeaponRolling(LowRoll).TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.Hit));
            Assert.That(_target.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
        }

        [Test]
        public void RollAboveHitChance_MissesButSpendsCooldown()
        {
            Weapon weapon = WeaponRolling(HighRoll);

            Assert.That(weapon.TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.Missed));
            Assert.That(_target.CurrentHealth, Is.EqualTo(TargetHealth));
            Assert.That(weapon.IsReady, Is.False);
        }

        [Test]
        public void Cover_LowersHitChance()
        {
            _target.Protection = new CombatProtection(HalfCover, 1f);

            Assert.That(WeaponRolling(LowRoll).TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.Missed));
        }

        [Test]
        public void DamageMultiplier_ReducesDamageOnHit()
        {
            _target.Protection = new CombatProtection(1f, GarrisonDamage);

            WeaponRolling(LowRoll).TryFire(Vector3.Zero, _target);

            Assert.That(_target.CurrentHealth, Is.EqualTo(TargetHealth - (Damage * GarrisonDamage)));
        }

        [Test]
        public void RangeBonus_AllowsShotBeyondBaseRange()
        {
            Weapon weapon = new Weapon(new WeaponSettings(Damage, Range, Cooldown));
            _target.Position = new Vector3(Range + 1f, 0f, 0f);
            Assert.That(weapon.TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.NotFired));

            weapon.SetRangeBonus(RangeBonus);

            Assert.That(weapon.EffectiveRange, Is.EqualTo(Range + RangeBonus));
            Assert.That(weapon.TryFire(Vector3.Zero, _target), Is.EqualTo(FireOutcome.Hit));
        }

        [Test]
        public void SetRangeBonus_RejectsNegative()
        {
            Weapon weapon = new Weapon(new WeaponSettings(Damage, Range, Cooldown));

            Assert.Throws<ArgumentOutOfRangeException>(() => weapon.SetRangeBonus(-1f));
        }

        [TestCase(-0.1f)]
        [TestCase(1.1f)]
        [TestCase(float.NaN)]
        public void WithHitChance_RejectsOutOfRange(float hitChance)
        {
            WeaponSettings settings = new WeaponSettings(Damage, Range, Cooldown);

            Assert.Throws<ArgumentOutOfRangeException>(() => settings.WithHitChance(hitChance));
        }

        [Test]
        public void WithHitChance_ReturnsNewInstanceKeepingStats()
        {
            WeaponSettings settings = new WeaponSettings(Damage, Range, Cooldown);

            WeaponSettings changed = settings.WithHitChance(HitChance);

            Assert.That(changed, Is.Not.SameAs(settings));
            Assert.That(settings.HitChance, Is.EqualTo(1f));
            Assert.That(changed.HitChance, Is.EqualTo(HitChance));
            Assert.That(changed.Damage, Is.EqualTo(Damage));
            Assert.That(changed.Range, Is.EqualTo(Range));
            Assert.That(changed.CooldownSeconds, Is.EqualTo(Cooldown));
        }

        [Test]
        public void SystemRandomSource_StaysInUnitInterval()
        {
            const int Samples = 1000;
            const int Seed = 42;
            SystemRandomSource random = new SystemRandomSource(Seed);

            for (int i = 0; i < Samples; i++)
            {
                float value = random.NextFloat();
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
        }
    }
}
