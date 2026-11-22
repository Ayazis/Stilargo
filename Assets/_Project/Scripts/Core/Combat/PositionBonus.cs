using System;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// What a unit gains from where it stands (a garrison slot, a cover point): extra weapon range,
    /// extra sight range and protection against incoming fire.
    /// </summary>
    public readonly struct PositionBonus
    {
        public PositionBonus(float rangeMetres, float sightMetres, CombatProtection protection)
        {
            RequireNonNegative(rangeMetres, nameof(rangeMetres));
            RequireNonNegative(sightMetres, nameof(sightMetres));
            RangeMetres = rangeMetres;
            SightMetres = sightMetres;
            Protection = protection;
        }

        public static PositionBonus None => new PositionBonus(0f, 0f, CombatProtection.None);

        public float RangeMetres { get; }

        public float SightMetres { get; }

        public CombatProtection Protection { get; }

        public PositionBonus Combine(PositionBonus other)
        {
            return new PositionBonus(RangeMetres + other.RangeMetres, SightMetres + other.SightMetres, Protection.Combine(other.Protection));
        }

        private static void RequireNonNegative(float value, string name)
        {
            if (!(value >= 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Bonus must be a non-negative finite number.");
            }
        }
    }
}
