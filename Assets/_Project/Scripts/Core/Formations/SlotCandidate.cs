namespace Stilargo.Core.Formations
{
    /// <summary>
    /// One possible unit-to-slot pairing considered by <see cref="SlotAssignment"/>.
    /// </summary>
    internal readonly struct SlotCandidate
    {
        public SlotCandidate(int unit, int slot, float distanceSquared)
        {
            Unit = unit;
            Slot = slot;
            DistanceSquared = distanceSquared;
        }

        public int Unit { get; }

        public int Slot { get; }

        public float DistanceSquared { get; }
    }
}
