using System;

namespace Stilargo.Core.City
{
    /// <summary>
    /// How far the outskirts stray from the plain grid: neighbouring blocks merged into one big block, and narrow
    /// alleys cut through others. From the <c>streets</c> section of <c>Town.json</c>.
    /// </summary>
    public sealed class StreetSettings
    {
        public const int DefaultMergedBlockPairs = 3;
        public const float DefaultAlleyChance = 0.5f;
        public const int DefaultAlleyWidthCells = 1;

        /// <summary>How many pairs of neighbouring outskirts blocks lose the street between them.</summary>
        public int MergedBlockPairs { get; set; } = DefaultMergedBlockPairs;

        /// <summary>Chance (0–1) that an outskirts block is cut in two by an alley.</summary>
        public float AlleyChance { get; set; } = DefaultAlleyChance;

        public int AlleyWidthCells { get; set; } = DefaultAlleyWidthCells;

        public StreetSettings Validate()
        {
            if (MergedBlockPairs < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MergedBlockPairs), MergedBlockPairs, "Must not be negative.");
            }

            Share.Require(AlleyChance, nameof(AlleyChance));
            if (AlleyWidthCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(AlleyWidthCells), AlleyWidthCells, "Must be positive.");
            }

            return this;
        }
    }
}
