using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Tests.EditMode.Combat;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class TurretTests
    {
        private const float Damage = 10f;
        private const float RangeMetres = 10f;
        private const float CooldownSeconds = 1f;
        private const float AlwaysHits = 0f;

        private FakeVisibilityQuery _visibility;
        private Turret _turret;

        [SetUp]
        public void SetUp()
        {
            _visibility = new FakeVisibilityQuery();
            Weapon weapon = new Weapon(new WeaponSettings(Damage, RangeMetres, CooldownSeconds), new FakeRandomSource(AlwaysHits));
            _turret = new Turret(weapon, Vector3.Zero, _visibility);
        }

        [Test]
        public void Tick_ShootsNearestVisibleEnemyInRange()
        {
            FakeCombatTarget far = new FakeCombatTarget(new Vector3(8f, 0f, 0f), 100f);
            FakeCombatTarget near = new FakeCombatTarget(new Vector3(4f, 0f, 0f), 100f);

            FireOutcome outcome = _turret.Tick(0f, new List<ICombatTarget> { far, near });

            Assert.AreEqual(FireOutcome.Hit, outcome);
            Assert.AreSame(near, _turret.LastTarget);
            Assert.AreEqual(100f - Damage, near.CurrentHealth);
            Assert.AreEqual(100f, far.CurrentHealth);
        }

        [Test]
        public void Tick_IgnoresEnemiesHiddenByFog()
        {
            FakeCombatTarget hidden = new FakeCombatTarget(new Vector3(3f, 0f, 0f), 100f);
            FakeCombatTarget visible = new FakeCombatTarget(new Vector3(6f, 0f, 0f), 100f);
            _visibility.Hide(hidden.Position);

            _turret.Tick(0f, new List<ICombatTarget> { hidden, visible });

            Assert.AreSame(visible, _turret.LastTarget);
        }

        [Test]
        public void Tick_EnemyOutOfRange_DoesNotFire()
        {
            FakeCombatTarget outOfRange = new FakeCombatTarget(new Vector3(RangeMetres + 1f, 0f, 0f), 100f);

            FireOutcome outcome = _turret.Tick(0f, new List<ICombatTarget> { outOfRange });

            Assert.AreEqual(FireOutcome.NotFired, outcome);
            Assert.IsNull(_turret.LastTarget);
        }

        [Test]
        public void Tick_WaitsForCooldownBetweenShots()
        {
            FakeCombatTarget enemy = new FakeCombatTarget(new Vector3(2f, 0f, 0f), 100f);
            List<ICombatTarget> enemies = new List<ICombatTarget> { enemy };

            _turret.Tick(0f, enemies);
            FireOutcome early = _turret.Tick(CooldownSeconds * 0.5f, enemies);
            FireOutcome ready = _turret.Tick(CooldownSeconds * 0.5f, enemies);

            Assert.AreEqual(FireOutcome.NotFired, early);
            Assert.AreEqual(FireOutcome.Hit, ready);
        }

        [Test]
        public void Tick_DuringCooldown_KeepsAimingAtTarget()
        {
            FakeCombatTarget enemy = new FakeCombatTarget(new Vector3(2f, 0f, 0f), 100f);
            List<ICombatTarget> enemies = new List<ICombatTarget> { enemy };

            _turret.Tick(0f, enemies);
            _turret.Tick(CooldownSeconds * 0.5f, enemies);

            Assert.IsNull(_turret.LastTarget);
            Assert.AreSame(enemy, _turret.AimTarget);
        }
    }
}
