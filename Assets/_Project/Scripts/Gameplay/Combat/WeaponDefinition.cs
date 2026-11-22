using Stilargo.Core.Combat;
using UnityEngine;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>Authorable weapon stats; Core only sees the resulting <see cref="WeaponSettings"/>.</summary>
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Stilargo/Combat/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        private const float DefaultDamage = 10f;
        private const float DefaultRangeMetres = 12f;
        private const float DefaultCooldownSeconds = 0.8f;
        private const float DefaultHitChance = 0.75f;

        [SerializeField, Min(0.01f)] private float _damage = DefaultDamage;
        [SerializeField, Min(0.01f)] private float _range = DefaultRangeMetres;
        [SerializeField, Min(0f)] private float _cooldownSeconds = DefaultCooldownSeconds;
        [SerializeField, Range(0f, 1f)] private float _hitChance = DefaultHitChance;

        public float Range => _range;

        public WeaponSettings ToSettings()
        {
            return new WeaponSettings(_damage, _range, _cooldownSeconds).WithHitChance(_hitChance);
        }
    }
}
