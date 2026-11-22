using System;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// Plain weapon stats. Gameplay's WeaponDefinition asset converts into this.
    /// </summary>
    public sealed class WeaponSettings
    {
        private const float CertainHit = 1f;

        public WeaponSettings(float damage, float range, float cooldownSeconds)
        {
            if (damage <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Damage must be positive.");
            }

            if (range <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(range), range, "Range must be positive.");
            }

            if (cooldownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds), cooldownSeconds, "Cooldown must not be negative.");
            }

            Damage = damage;
            Range = range;
            CooldownSeconds = cooldownSeconds;
            HitChance = CertainHit;
        }

        private WeaponSettings(WeaponSettings source, float hitChance)
        {
            Damage = source.Damage;
            Range = source.Range;
            CooldownSeconds = source.CooldownSeconds;
            HitChance = hitChance;
        }

        public float Damage { get; }

        /// <summary>Maximum firing distance in metres, measured on the ground plane.</summary>
        public float Range { get; }

        public float CooldownSeconds { get; }

        /// <summary>Chance in [0, 1] that a shot at an unprotected target hits.</summary>
        public float HitChance { get; }

        public WeaponSettings WithHitChance(float hitChance)
        {
            if (!(hitChance >= 0f && hitChance <= 1f))
            {
                throw new ArgumentOutOfRangeException(nameof(hitChance), hitChance, "Hit chance must be in [0, 1].");
            }

            return new WeaponSettings(this, hitChance);
        }
    }
}
