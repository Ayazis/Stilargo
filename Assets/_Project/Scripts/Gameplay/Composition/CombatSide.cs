using System;
using Stilargo.Gameplay.Combat;
using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Tuning shared by every unit of one side, set on the installer in the scene.
    /// </summary>
    [Serializable]
    public sealed class CombatSide
    {
        private const float DefaultMaxHealth = 100f;
        private const float DefaultSightRangeMetres = 18f;

        [SerializeField] private WeaponDefinition _weapon;
        [SerializeField, Min(1f)] private float _maxHealth = DefaultMaxHealth;
        [SerializeField, Min(0.1f)] private float _sightRange = DefaultSightRangeMetres;

        public WeaponDefinition Weapon => _weapon;

        public float MaxHealth => _maxHealth;

        /// <summary>How far units notice enemies; must be at least the weapon's range.</summary>
        public float SightRange => _sightRange;
    }
}
