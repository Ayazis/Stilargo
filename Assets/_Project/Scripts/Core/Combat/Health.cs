using System;

namespace Stilargo.Core.Combat
{
    public sealed class Health : IDamageable
    {
        public Health(float maxHealth)
        {
            if (!(maxHealth > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Max health must be positive.");
            }

            Max = maxHealth;
            Current = maxHealth;
        }

        public event Action Died;

        /// <summary>Raised with the health actually lost, after it was applied.</summary>
        public event Action<float> Damaged;

        public float Max { get; }

        public float Current { get; private set; }

        public float Fraction => Current / Max;

        public bool IsAlive => Current > 0f;

        public void TakeDamage(float amount)
        {
            if (float.IsNaN(amount) || amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Damage must be a non-negative number.");
            }

            if (!IsAlive)
            {
                return;
            }

            float applied = Math.Min(amount, Current);
            Current -= applied;
            Damaged?.Invoke(applied);

            if (!IsAlive)
            {
                Died?.Invoke();
            }
        }
    }
}
