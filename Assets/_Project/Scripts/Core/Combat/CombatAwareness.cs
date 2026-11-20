using System;

namespace Stilargo.Core.Combat
{
    /// <summary>How far a unit notices enemies, in metres on the ground plane.</summary>
    public sealed class CombatAwareness
    {
        public CombatAwareness(float sightRange)
        {
            if (!(sightRange > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(sightRange), sightRange, "Sight range must be positive.");
            }

            SightRange = sightRange;
        }

        public float SightRange { get; }
    }
}
