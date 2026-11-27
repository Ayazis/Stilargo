using System;
using Stilargo.Core.Combat;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>player</c> or <c>enemy</c> section of <c>Combat.json</c>.</summary>
    [Serializable]
    public sealed class UnitSideEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float maxHealth;
        [SerializeField] private float weaponDamage;
        [SerializeField] private float weaponRangeMetres;
        [SerializeField] private float weaponCooldownSeconds;
        [SerializeField] private float weaponHitChance;
        [SerializeField] private float sightRangeMetres;

        public UnitSideEntry()
            : this(new CombatSettings().Player)
        {
        }

        private UnitSideEntry(UnitSideSettings defaults)
        {
            maxHealth = defaults.MaxHealth;
            weaponDamage = defaults.Weapon.Damage;
            weaponRangeMetres = defaults.Weapon.Range;
            weaponCooldownSeconds = defaults.Weapon.CooldownSeconds;
            weaponHitChance = defaults.Weapon.HitChance;
            sightRangeMetres = defaults.SightRangeMetres;
        }

        public static UnitSideEntry DefaultsFor(UnitSideSettings defaults)
        {
            if (defaults == null)
            {
                throw new ArgumentNullException(nameof(defaults));
            }

            return new UnitSideEntry(defaults);
        }

        public UnitSideSettings ToSettings(string sectionName)
        {
            try
            {
                var weapon = new WeaponSettings(weaponDamage, weaponRangeMetres, weaponCooldownSeconds).WithHitChance(weaponHitChance);
                return new UnitSideSettings(maxHealth, weapon, sightRangeMetres);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"{sectionName}: {exception.Message}", exception);
            }
        }
    }
}
