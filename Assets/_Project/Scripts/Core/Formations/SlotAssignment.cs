using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Matches units to formation slots so paths rarely cross: repeatedly pairs the closest free unit
    /// and free slot (greedy, ground-plane distance). Not optimal, but cheap and good enough for RTS groups.
    /// </summary>
    public sealed class SlotAssignment
    {
        /// <returns>For each position, the index of its slot in <paramref name="slots"/>. No slot is used twice.</returns>
        public IReadOnlyList<int> Assign(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector3> slots)
        {
            ThrowIfInvalid(positions, slots);

            var assigned = new int[positions.Count];
            var isUnitDone = new bool[positions.Count];
            var isSlotTaken = new bool[slots.Count];

            foreach (SlotCandidate candidate in CandidatesByDistance(positions, slots))
            {
                if (isUnitDone[candidate.Unit] || isSlotTaken[candidate.Slot])
                {
                    continue;
                }

                assigned[candidate.Unit] = candidate.Slot;
                isUnitDone[candidate.Unit] = true;
                isSlotTaken[candidate.Slot] = true;
            }

            return assigned;
        }

        private static void ThrowIfInvalid(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector3> slots)
        {
            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            if (slots.Count < positions.Count)
            {
                throw new ArgumentException($"Need at least {positions.Count} slots but got {slots.Count}.", nameof(slots));
            }
        }

        private static List<SlotCandidate> CandidatesByDistance(IReadOnlyList<Vector3> positions, IReadOnlyList<Vector3> slots)
        {
            var candidates = new List<SlotCandidate>(positions.Count * slots.Count);
            for (int unit = 0; unit < positions.Count; unit++)
            {
                for (int slot = 0; slot < slots.Count; slot++)
                {
                    float distance = GroundPlane.DistanceSquared(positions[unit], slots[slot]);
                    candidates.Add(new SlotCandidate(unit, slot, distance));
                }
            }

            candidates.Sort((a, b) => a.DistanceSquared.CompareTo(b.DistanceSquared));
            return candidates;
        }
    }
}
