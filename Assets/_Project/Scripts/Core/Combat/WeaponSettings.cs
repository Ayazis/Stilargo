using System;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// Plain weapon stats. Gameplay's WeaponDefinition asset converts into this.
    /// </summary>
    public sealed class WeaponSettings
    {
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
        }

        public float Damage { get; }

        /// <summary>Maximum firing distance in metres, measured on the ground plane.</summary>
        public float Range { get; }

        public float CooldownSeconds { get; }
    }
}
