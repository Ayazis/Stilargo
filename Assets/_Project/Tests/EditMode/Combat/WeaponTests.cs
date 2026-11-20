using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class WeaponTests
    {
        private const float Damage = 10f;
        private const float Range = 5f;
        private const float Cooldown = 2f;
        private const float TargetHealth = 100f;

        private Weapon _weapon;
        private FakeCombatTarget _target;

        [SetUp]
        public void SetUp()
        {
            _weapon = new Weapon(new WeaponSettings(Damage, Range, Cooldown));
            _target = new FakeCombatTarget(new Vector3(3f, 0f, 0f), TargetHealth);
        }

        [Test]
        public void NewWeapon_IsReady()
        {
            Assert.That(_weapon.IsReady, Is.True);
        }

        [Test]
        public void TryFire_InRange_AppliesDamageAndStartsCooldown()
        {
            bool hasFired = _weapon.TryFire(Vector3.Zero, _target);

            Assert.That(hasFired, Is.True);
            Assert.That(_target.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
            Assert.That(_weapon.IsReady, Is.False);
        }

        [Test]
        public void TryFire_DuringCooldown_DoesNotFire()
        {
            _weapon.TryFire(Vector3.Zero, _target);
            _weapon.Tick(Cooldown / 2f);

            Assert.That(_weapon.TryFire(Vector3.Zero, _target), Is.False);
            Assert.That(_target.CurrentHealth, Is.EqualTo(TargetHealth - Damage));
        }

        [Test]
        public void Tick_PastCooldown_MakesReadyAndCooldownRestartsOnFire()
        {
            _weapon.TryFire(Vector3.Zero, _target);
            _weapon.Tick(Cooldown);

            Assert.That(_weapon.IsReady, Is.True);
            Assert.That(_weapon.TryFire(Vector3.Zero, _target), Is.True);
            Assert.That(_weapon.IsReady, Is.False);
        }

        [Test]
        public void IsInRange_IsInclusiveAndIgnoresHeight()
        {
            Assert.That(_weapon.IsInRange(Vector3.Zero, new Vector3(Range, 50f, 0f)), Is.True);
            Assert.That(_weapon.IsInRange(Vector3.Zero, new Vector3(Range + 0.01f, 0f, 0f)), Is.False);
        }

        [Test]
        public void TryFire_OutOfRange_DoesNotFire()
        {
            _target.Position = new Vector3(0f, 0f, Range + 1f);

            Assert.That(_weapon.TryFire(Vector3.Zero, _target), Is.False);
            Assert.That(_weapon.IsReady, Is.True);
        }

        [Test]
        public void TryFire_AtDeadTarget_DoesNotFire()
        {
            _target.TakeDamage(TargetHealth);

            Assert.That(_weapon.TryFire(Vector3.Zero, _target), Is.False);
            Assert.That(_weapon.IsReady, Is.True);
        }
    }
}
