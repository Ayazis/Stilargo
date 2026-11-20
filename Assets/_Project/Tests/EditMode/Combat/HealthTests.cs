using System;
using NUnit.Framework;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class HealthTests
    {
        private const float MaxHealth = 100f;

        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _health = new Health(MaxHealth);
        }

        [Test]
        public void NewHealth_IsFullAndAlive()
        {
            Assert.That(_health.Current, Is.EqualTo(MaxHealth));
            Assert.That(_health.Max, Is.EqualTo(MaxHealth));
            Assert.That(_health.Fraction, Is.EqualTo(1f));
            Assert.That(_health.IsAlive, Is.True);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Constructor_RejectsNonPositiveMax(float maxHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(maxHealth));
        }

        [Test]
        public void TakeDamage_ReducesCurrentAndRaisesDamaged()
        {
            float reported = 0f;
            _health.Damaged += amount => reported = amount;

            _health.TakeDamage(30f);

            Assert.That(_health.Current, Is.EqualTo(70f));
            Assert.That(_health.Fraction, Is.EqualTo(0.7f).Within(1e-5f));
            Assert.That(reported, Is.EqualTo(30f));
        }

        [Test]
        public void TakeDamage_BeyondCurrent_ClampsAtZeroAndDies()
        {
            int diedCount = 0;
            _health.Died += () => diedCount++;

            _health.TakeDamage(250f);

            Assert.That(_health.Current, Is.EqualTo(0f));
            Assert.That(_health.IsAlive, Is.False);
            Assert.That(diedCount, Is.EqualTo(1));
        }

        [Test]
        public void TakeDamage_AfterDeath_IsIgnoredAndDiedNotRaisedAgain()
        {
            int diedCount = 0;
            int damagedCount = 0;
            _health.Died += () => diedCount++;
            _health.TakeDamage(MaxHealth);
            _health.Damaged += _ => damagedCount++;

            _health.TakeDamage(10f);

            Assert.That(_health.Current, Is.EqualTo(0f));
            Assert.That(diedCount, Is.EqualTo(1));
            Assert.That(damagedCount, Is.EqualTo(0));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void TakeDamage_RejectsInvalidAmounts(float amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _health.TakeDamage(amount));
            Assert.That(_health.Current, Is.EqualTo(MaxHealth));
        }
    }
}
