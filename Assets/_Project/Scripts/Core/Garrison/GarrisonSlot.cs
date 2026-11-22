using System;
using System.Numerics;

namespace Stilargo.Core.Garrison
{
    /// <summary>One spot in a building that a single unit can occupy.</summary>
    public sealed class GarrisonSlot
    {
        public GarrisonSlot(GarrisonSlotKind kind, int storey, GarrisonSlotPose pose)
        {
            if (storey < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storey), storey, "Storey must not be negative.");
            }

            Kind = kind;
            Storey = storey;
            Position = pose.Position;
            Facing = pose.Facing;
        }

        public GarrisonSlotKind Kind { get; }

        /// <summary>0-based storey; a roof slot's storey equals the building's storey count.</summary>
        public int Storey { get; }

        /// <summary>Where the unit stands, in world metres.</summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Horizontal unit vector away from the building. Roof slots face the building's first road-facing
        /// facade so a rooftop unit looks over the street rather than at the sky.
        /// </summary>
        public Vector3 Facing { get; }

        public bool IsOccupied => Occupant != null;

        public object Occupant { get; private set; }

        internal void Occupy(object occupant)
        {
            Occupant = occupant;
        }

        internal void Vacate()
        {
            Occupant = null;
        }
    }
}
