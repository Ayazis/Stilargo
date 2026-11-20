using System;
using System.Numerics;

namespace Stilargo.Core.Combat
{
    public sealed class Weapon
    {
        private float _remainingCooldown;

        public Weapon(WeaponSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public WeaponSettings Settings { get; }

        public bool IsReady => _remainingCooldown <= 0f;

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be a non-negative number.");
            }

            _remainingCooldown = Math.Max(0f, _remainingCooldown - deltaSeconds);
        }

        public bool IsInRange(Vector3 from, Vector3 to)
        {
            return GroundDistance.Between(from, to) <= Settings.Range;
        }

        public bool TryFire(Vector3 shooterPosition, ICombatTarget target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!IsReady || !target.IsAlive || !IsInRange(shooterPosition, target.Position))
            {
                return false;
            }

            target.TakeDamage(Settings.Damage);
            _remainingCooldown = Settings.CooldownSeconds;
            return true;
        }
    }
}
