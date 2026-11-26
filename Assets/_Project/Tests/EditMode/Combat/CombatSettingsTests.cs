using System;
using NUnit.Framework;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class CombatSettingsTests
    {
        [Test]
        public void Defaults_LetAnEnemyLastLongerThanTwentySecondsAgainstOneSoldier()
        {
            var settings = new CombatSettings();
            WeaponSettings rifle = settings.Player.Weapon;
            float damagePerSecond = rifle.Damage * rifle.HitChance / rifle.CooldownSeconds;

            Assert.Greater(settings.Enemy.MaxHealth / damagePerSecond, 20f);
        }

        [Test]
        public void UnitSideSettings_RejectsSightShorterThanTheWeapon()
        {
            var weapon = new WeaponSettings(5f, 14f, 1f);

            Assert.That(() => new UnitSideSettings(100f, weapon, 10f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void UnitSideSettings_RejectsNonPositiveHealth()
        {
            var weapon = new WeaponSettings(5f, 14f, 1f);

            Assert.That(() => new UnitSideSettings(0f, weapon, 20f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void WithPlayerMoveSpeed_RejectsZero()
        {
            Assert.That(() => new CombatSettings().WithPlayerMoveSpeed(0f), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void WithCorpseTime_AllowsZero()
        {
            Assert.AreEqual(0f, new CombatSettings().WithCorpseTime(0f).CorpseSeconds);
        }
    }
}
