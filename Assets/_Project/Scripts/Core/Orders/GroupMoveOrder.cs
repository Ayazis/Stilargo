using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Formations;
using Stilargo.Core.Selection;

namespace Stilargo.Core.Orders
{
    /// <summary>
    /// Sends a group of units to one destination, each to its own formation slot.
    /// The formation faces from the group's centroid toward the destination.
    /// </summary>
    public sealed class GroupMoveOrder
    {
        private readonly SlotAssignment _assignment = new SlotAssignment();
        private IFormation _formation;

        public GroupMoveOrder(IFormation formation)
        {
            Formation = formation;
        }

        /// <summary>The formation used by the next order. Swap it to change formation.</summary>
        public IFormation Formation
        {
            get => _formation;
            set => _formation = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>Replaces each unit's orders with a move to its slot.</summary>
        public void Move(IReadOnlyList<ISelectable> units, Vector3 destination)
        {
            IReadOnlyList<Vector3> slots = SlotPerUnit(units, destination);

            for (int i = 0; i < slots.Count; i++)
            {
                units[i].Orders.MoveTo(slots[i]);
            }
        }

        /// <summary>Appends a move to its slot to each unit's orders.</summary>
        public void QueueMove(IReadOnlyList<ISelectable> units, Vector3 destination)
        {
            IReadOnlyList<Vector3> slots = SlotPerUnit(units, destination);

            for (int i = 0; i < slots.Count; i++)
            {
                units[i].Orders.QueueMoveTo(slots[i]);
            }
        }

        /// <returns>The slot for each unit, in the same order as <paramref name="units"/>.</returns>
        private IReadOnlyList<Vector3> SlotPerUnit(IReadOnlyList<ISelectable> units, Vector3 destination)
        {
            IReadOnlyList<Vector3> positions = PositionsOf(units);

            if (positions.Count == 0)
            {
                return Array.Empty<Vector3>();
            }

            Vector3 facing = destination - Centroid(positions);
            IReadOnlyList<Vector3> slots = _formation.SlotsFor(positions.Count, destination, facing);
            IReadOnlyList<int> slotIndices = _assignment.Assign(positions, slots);

            var unitSlots = new Vector3[positions.Count];
            for (int i = 0; i < unitSlots.Length; i++)
            {
                unitSlots[i] = slots[slotIndices[i]];
            }

            return unitSlots;
        }

        private static IReadOnlyList<Vector3> PositionsOf(IReadOnlyList<ISelectable> units)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            var positions = new Vector3[units.Count];
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] == null || units[i].Orders == null)
                {
                    throw new ArgumentException($"Unit {i} is null or has no orders.", nameof(units));
                }

                positions[i] = units[i].Position;
            }

            return positions;
        }

        private static Vector3 Centroid(IReadOnlyList<Vector3> positions)
        {
            Vector3 sum = Vector3.Zero;
            foreach (Vector3 position in positions)
            {
                sum += position;
            }

            return sum / positions.Count;
        }
    }
}
