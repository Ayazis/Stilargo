using System;

namespace Stilargo.Core.City
{
    /// <summary>How busy the streets look: lamps, street trees and clutter. From the <c>streetLife</c> section of <c>Town.json</c>.</summary>
    public sealed class StreetLifeSettings
    {
        public const int DefaultLampSpacingCells = 7;
        public const int DefaultTreeSpacingCells = 3;
        public const float DefaultClutterChance = 0.12f;

        /// <summary>A lamp on every this many cells of pavement along a building.</summary>
        public int LampSpacingCells { get; set; } = DefaultLampSpacingCells;

        /// <summary>A tree on every this many cells of pavement along a park, garden or square.</summary>
        public int TreeSpacingCells { get; set; } = DefaultTreeSpacingCells;

        /// <summary>Chance (0–1) that a cell of pavement along a building gets crates, barrels, a planter or a cart.</summary>
        public float ClutterChance { get; set; } = DefaultClutterChance;

        public StreetLifeSettings Validate()
        {
            if (LampSpacingCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(LampSpacingCells), LampSpacingCells, "Must be positive.");
            }

            if (TreeSpacingCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(TreeSpacingCells), TreeSpacingCells, "Must be positive.");
            }

            Share.Require(ClutterChance, nameof(ClutterChance));
            return this;
        }
    }
}
