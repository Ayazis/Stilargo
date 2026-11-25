using System;
using System.Numerics;

namespace Stilargo.Core.Fortification
{
    /// <summary>One spot on the claimed house that holds at most one fortification.</summary>
    public sealed class FortificationSlot
    {
        public FortificationSlot(FortificationSlotKind kind, Vector3 position, Vector3 facing)
        {
            if (facing.LengthSquared() <= 0f)
            {
                throw new ArgumentException("Facing must not be zero.", nameof(facing));
            }

            Kind = kind;
            Position = position;
            Facing = Vector3.Normalize(facing);
        }

        public FortificationSlotKind Kind { get; }

        /// <summary>World metres.</summary>
        public Vector3 Position { get; }

        /// <summary>Horizontal unit vector pointing away from the house.</summary>
        public Vector3 Facing { get; }

        /// <summary>The fortification built here, or null while the slot is empty.</summary>
        public FortificationDefinition Fortification { get; private set; }

        public bool IsFilled => Fortification != null;

        internal void Fill(FortificationDefinition fortification)
        {
            if (IsFilled)
            {
                throw new InvalidOperationException("Slot already holds a fortification.");
            }

            Fortification = fortification;
        }
    }
}
