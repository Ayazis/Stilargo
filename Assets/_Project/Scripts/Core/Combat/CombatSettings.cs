using System;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// Tunable values for unit combat, loaded from <c>Combat.json</c>. Defaults give fights that last long enough
    /// to react to: a lone enemy takes about half a minute to bring down, a lone soldier over a minute.
    /// </summary>
    public sealed class CombatSettings
    {
        public const float DefaultPlayerMaxHealth = 250f;
        public const float DefaultPlayerDamage = 10f;
        public const float DefaultPlayerCooldownSeconds = 1.1f;
        public const float DefaultPlayerHitChance = 0.6f;
        public const float DefaultEnemyMaxHealth = 160f;
        public const float DefaultEnemyDamage = 7f;
        public const float DefaultEnemyCooldownSeconds = 1.2f;
        public const float DefaultEnemyHitChance = 0.55f;
        public const float DefaultWeaponRangeMetres = 14f;
        public const float DefaultSightRangeMetres = 20f;
        public const float DefaultPlayerMoveSpeedMetresPerSecond = 3.8f;
        public const float DefaultCorpseSeconds = 6f;

        public UnitSideSettings Player { get; private set; } = new UnitSideSettings(
            DefaultPlayerMaxHealth,
            new WeaponSettings(DefaultPlayerDamage, DefaultWeaponRangeMetres, DefaultPlayerCooldownSeconds).WithHitChance(DefaultPlayerHitChance),
            DefaultSightRangeMetres);

        public UnitSideSettings Enemy { get; private set; } = new UnitSideSettings(
            DefaultEnemyMaxHealth,
            new WeaponSettings(DefaultEnemyDamage, DefaultWeaponRangeMetres, DefaultEnemyCooldownSeconds).WithHitChance(DefaultEnemyHitChance),
            DefaultSightRangeMetres);

        /// <summary>Top speed of the player's soldiers. The rival's speed comes from <c>Hill.json</c>.</summary>
        public float PlayerMoveSpeedMetresPerSecond { get; private set; } = DefaultPlayerMoveSpeedMetresPerSecond;

        /// <summary>How long a fallen soldier stays on the ground before it is cleared away.</summary>
        public float CorpseSeconds { get; private set; } = DefaultCorpseSeconds;

        public CombatSettings WithSides(UnitSideSettings player, UnitSideSettings enemy)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            return this;
        }

        public CombatSettings WithPlayerMoveSpeed(float metresPerSecond)
        {
            PlayerMoveSpeedMetresPerSecond = RequirePositive(metresPerSecond, nameof(metresPerSecond));
            return this;
        }

        public CombatSettings WithCorpseTime(float seconds)
        {
            if (!(seconds >= 0f) || float.IsInfinity(seconds))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), seconds, "Must be a non-negative number.");
            }

            CorpseSeconds = seconds;
            return this;
        }

        private static float RequirePositive(float value, string name)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be a positive number.");
            }

            return value;
        }
    }
}
