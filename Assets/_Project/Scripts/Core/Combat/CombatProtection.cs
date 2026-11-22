using System;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// How well a target is protected: multipliers on the shooter's hit chance (cover) and on the
    /// damage it takes (garrison walls). 1 means no protection.
    /// </summary>
    public readonly struct CombatProtection
    {
        public CombatProtection(float hitChanceMultiplier, float damageMultiplier)
        {
            RequireMultiplier(hitChanceMultiplier, nameof(hitChanceMultiplier));
            RequireMultiplier(damageMultiplier, nameof(damageMultiplier));
            HitChanceMultiplier = hitChanceMultiplier;
            DamageMultiplier = damageMultiplier;
        }

        public static CombatProtection None => new CombatProtection(1f, 1f);

        /// <summary>In [0, 1].</summary>
        public float HitChanceMultiplier { get; }

        /// <summary>In [0, 1].</summary>
        public float DamageMultiplier { get; }

        public CombatProtection Combine(CombatProtection other)
        {
            return new CombatProtection(HitChanceMultiplier * other.HitChanceMultiplier, DamageMultiplier * other.DamageMultiplier);
        }

        private static void RequireMultiplier(float value, string name)
        {
            if (!(value >= 0f && value <= 1f))
            {
                throw new ArgumentOutOfRangeException(name, value, "Protection multipliers must be in [0, 1].");
            }
        }
    }
}
