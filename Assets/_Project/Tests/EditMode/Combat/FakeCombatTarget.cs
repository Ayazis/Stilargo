using System;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    public sealed class FakeCombatTarget : ICombatTarget
    {
        private readonly Health _health;

        public FakeCombatTarget(Vector3 position, float maxHealth)
        {
            Position = position;
            _health = new Health(maxHealth);
        }

        public event Action Died
        {
            add
            {
                _health.Died += value;
            }

            remove
            {
                _health.Died -= value;
            }
        }

        public Vector3 Position { get; set; }

        public float HealthFraction => _health.Fraction;

        public bool IsAlive => _health.IsAlive;

        public float CurrentHealth => _health.Current;

        public void TakeDamage(float amount)
        {
            _health.TakeDamage(amount);
        }
    }
}
