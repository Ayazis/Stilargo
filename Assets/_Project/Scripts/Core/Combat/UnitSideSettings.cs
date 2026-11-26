using System;

namespace Stilargo.Core.Combat
{
    /// <summary>Health, weapon and sight shared by every unit of one side, loaded from <c>Combat.json</c>.</summary>
    public sealed class UnitSideSettings
    {
        public UnitSideSettings(float maxHealth, WeaponSettings weapon, float sightRangeMetres)
        {
            if (!(maxHealth > 0f) || float.IsInfinity(maxHealth))
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Max health must be a positive number.");
            }

            Weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            if (!(sightRangeMetres >= weapon.Range) || float.IsInfinity(sightRangeMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(sightRangeMetres), sightRangeMetres, "Sight range must be at least the weapon range.");
            }

            MaxHealth = maxHealth;
            SightRangeMetres = sightRangeMetres;
        }

        public float MaxHealth { get; }

        public WeaponSettings Weapon { get; }

        /// <summary>How far units notice enemies, in metres; at least the weapon's range.</summary>
        public float SightRangeMetres { get; }
    }
}
