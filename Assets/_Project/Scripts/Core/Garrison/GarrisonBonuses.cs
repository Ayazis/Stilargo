using System;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Garrison
{
    /// <summary>What standing in each kind of garrison slot is worth in combat.</summary>
    public sealed class GarrisonBonuses
    {
        public const float DefaultWindowRangeMetres = 3f;
        public const float DefaultWindowSightMetres = 3f;
        public const float DefaultWindowDamageMultiplier = 0.6f;
        public const float DefaultRoofRangeMetres = 5f;
        public const float DefaultRoofSightMetres = 8f;
        public const float DefaultRoofDamageMultiplier = 0.7f;

        private const float NoHitChanceReduction = 1f;

        public GarrisonBonuses()
            : this(
                new PositionBonus(DefaultWindowRangeMetres, DefaultWindowSightMetres, new CombatProtection(NoHitChanceReduction, DefaultWindowDamageMultiplier)),
                new PositionBonus(DefaultRoofRangeMetres, DefaultRoofSightMetres, new CombatProtection(NoHitChanceReduction, DefaultRoofDamageMultiplier)))
        {
        }

        public GarrisonBonuses(PositionBonus window, PositionBonus roof)
        {
            Window = window;
            Roof = roof;
        }

        public PositionBonus Window { get; }

        public PositionBonus Roof { get; }

        public PositionBonus For(GarrisonSlotKind kind)
        {
            switch (kind)
            {
                case GarrisonSlotKind.Window:
                    return Window;
                case GarrisonSlotKind.Roof:
                    return Roof;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown garrison slot kind.");
            }
        }

        public PositionBonus For(GarrisonSlot slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }

            return For(slot.Kind);
        }
    }
}
