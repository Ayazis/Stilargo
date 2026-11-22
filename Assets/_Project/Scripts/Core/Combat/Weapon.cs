using System;
using System.Numerics;

namespace Stilargo.Core.Combat
{
    public sealed class Weapon
    {
        private readonly IRandomSource _random;
        private float _remainingCooldown;
        private float _rangeBonus;

        public Weapon(WeaponSettings settings)
            : this(settings, new SystemRandomSource())
        {
        }

        public Weapon(WeaponSettings settings, IRandomSource random)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public WeaponSettings Settings { get; }

        public bool IsReady => _remainingCooldown <= 0f;

        /// <summary>Base range plus the current position bonus, in metres.</summary>
        public float EffectiveRange => Settings.Range + _rangeBonus;

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be a non-negative number.");
            }

            _remainingCooldown = Math.Max(0f, _remainingCooldown - deltaSeconds);
        }

        public void SetRangeBonus(float metres)
        {
            _rangeBonus = BonusMetres.Require(metres, nameof(metres));
        }

        public bool IsInRange(Vector3 from, Vector3 to)
        {
            return GroundDistance.Between(from, to) <= EffectiveRange;
        }

        /// <summary>A ready weapon with a living target in range always spends its cooldown, hit or miss.</summary>
        public FireOutcome TryFire(Vector3 shooterPosition, ICombatTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!IsReady || !target.IsAlive || !IsInRange(shooterPosition, target.Position))
            {
                return FireOutcome.NotFired;
            }

            _remainingCooldown = Settings.CooldownSeconds;
            CombatProtection protection = target.Protection;

            if (_random.NextFloat() >= Settings.HitChance * protection.HitChanceMultiplier)
            {
                return FireOutcome.Missed;
            }

            target.TakeDamage(Settings.Damage * protection.DamageMultiplier);
            return FireOutcome.Hit;
        }
    }
}
