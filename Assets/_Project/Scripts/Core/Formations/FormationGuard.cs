using System;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Shared argument checks so every formation rejects bad input the same way.
    /// </summary>
    internal static class FormationGuard
    {
        public static float PositiveSpacing(float spacing)
        {
            if (float.IsFinite(spacing) && spacing > 0f)
            {
                return spacing;
            }

            throw new ArgumentOutOfRangeException(nameof(spacing), spacing, "Must be a finite number greater than zero.");
        }

        public static void NonNegativeCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Must be zero or more.");
            }
        }
    }
}
