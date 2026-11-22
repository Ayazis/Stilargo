using System;
using NUnit.Framework;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class PositionBonusTests
    {
        [TestCase(-0.1f, 1f)]
        [TestCase(1f, 1.1f)]
        [TestCase(float.NaN, 1f)]
        public void CombatProtection_RejectsMultipliersOutsideUnitRange(float hitChance, float damage)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CombatProtection(hitChance, damage));
        }

        [Test]
        public void CombatProtection_CombineMultiplies()
        {
            CombatProtection combined = new CombatProtection(0.5f, 0.8f).Combine(new CombatProtection(0.5f, 0.5f));

            Assert.That(combined.HitChanceMultiplier, Is.EqualTo(0.25f));
            Assert.That(combined.DamageMultiplier, Is.EqualTo(0.4f));
        }

        [Test]
        public void CombatProtection_NoneIsNeutral()
        {
            Assert.That(CombatProtection.None.HitChanceMultiplier, Is.EqualTo(1f));
            Assert.That(CombatProtection.None.DamageMultiplier, Is.EqualTo(1f));
        }

        [TestCase(-1f, 0f)]
        [TestCase(0f, -1f)]
        [TestCase(float.PositiveInfinity, 0f)]
        [TestCase(0f, float.NaN)]
        public void PositionBonus_RejectsInvalidMetres(float range, float sight)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PositionBonus(range, sight, CombatProtection.None));
        }

        [Test]
        public void PositionBonus_CombineAddsMetresAndMultipliesProtection()
        {
            PositionBonus first = new PositionBonus(2f, 3f, new CombatProtection(0.5f, 1f));
            PositionBonus second = new PositionBonus(1f, 4f, new CombatProtection(0.5f, 0.5f));

            PositionBonus combined = first.Combine(second);

            Assert.That(combined.RangeMetres, Is.EqualTo(3f));
            Assert.That(combined.SightMetres, Is.EqualTo(7f));
            Assert.That(combined.Protection.HitChanceMultiplier, Is.EqualTo(0.25f));
            Assert.That(combined.Protection.DamageMultiplier, Is.EqualTo(0.5f));
        }
    }
}
