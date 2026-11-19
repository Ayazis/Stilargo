using System;

namespace Stilargo.Core.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        event Action Died;

        void TakeDamage(float amount);
    }
}
