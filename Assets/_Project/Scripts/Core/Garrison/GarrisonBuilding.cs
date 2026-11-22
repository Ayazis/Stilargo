using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.City;

namespace Stilargo.Core.Garrison
{
    /// <summary>
    /// A building units can enter. Free slots are handed out in list order; the generator lists roof slots
    /// first because rooftops give the best range and sight, then windows from the ground storey up.
    /// </summary>
    public sealed class GarrisonBuilding
    {
        private readonly List<GarrisonSlot> _slots;

        public GarrisonBuilding(Plot plot, IReadOnlyList<GarrisonSlot> slots, Vector3 entrance)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            if (slots.Any(slot => slot == null))
            {
                throw new ArgumentException("Slots must not contain null.", nameof(slots));
            }

            Plot = plot ?? throw new ArgumentNullException(nameof(plot));
            _slots = slots.ToList();
            Entrance = entrance;
        }

        public event Action<GarrisonSlot> SlotOccupied;

        public event Action<GarrisonSlot> SlotReleased;

        public Plot Plot { get; }

        public IReadOnlyList<GarrisonSlot> Slots => _slots;

        /// <summary>Road cell centre (Y = 0) a unit walks to before entering.</summary>
        public Vector3 Entrance { get; }

        public bool HasFreeSlot => _slots.Any(slot => !slot.IsOccupied);

        /// <summary>
        /// Puts the occupant in the first free slot; false when the building is full.
        /// Throws when the occupant already holds a slot here, since that is a caller bug.
        /// </summary>
        public bool TryOccupy(object occupant, out GarrisonSlot slot)
        {
            if (Contains(occupant))
            {
                throw new InvalidOperationException("Occupant already holds a slot in this building.");
            }

            slot = _slots.FirstOrDefault(candidate => !candidate.IsOccupied);
            if (slot == null)
            {
                return false;
            }

            slot.Occupy(occupant);
            SlotOccupied?.Invoke(slot);
            return true;
        }

        public bool Release(object occupant)
        {
            if (!TryGetSlotOf(occupant, out GarrisonSlot slot))
            {
                return false;
            }

            slot.Vacate();
            SlotReleased?.Invoke(slot);
            return true;
        }

        public bool Contains(object occupant)
        {
            return TryGetSlotOf(occupant, out _);
        }

        public bool TryGetSlotOf(object occupant, out GarrisonSlot slot)
        {
            if (occupant == null)
            {
                throw new ArgumentNullException(nameof(occupant));
            }

            slot = _slots.FirstOrDefault(candidate => ReferenceEquals(candidate.Occupant, occupant));
            return slot != null;
        }
    }
}
