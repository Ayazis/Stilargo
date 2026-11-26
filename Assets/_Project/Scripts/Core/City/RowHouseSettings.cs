using System;

namespace Stilargo.Core.City
{
    /// <summary>
    /// How the house blocks around the market square are built: tall town houses in a ring along the block's
    /// streets with a courtyard behind them. Loaded from the <c>houseBlock</c> section of <c>Town.json</c>.
    /// </summary>
    public sealed class RowHouseSettings
    {
        public const int DefaultRowDepthCells = 4;
        public const int DefaultMinFrontageCells = 3;
        public const int DefaultMaxFrontageCells = 5;
        public const int DefaultMinStoreys = 3;
        public const int DefaultMaxStoreys = 4;
        public const int DefaultCornerExtraStoreys = 1;
        private const int SplitHalves = 2;

        public RowHouseSettings()
            : this(
                DefaultRowDepthCells,
                new IntRange(DefaultMinFrontageCells, DefaultMaxFrontageCells),
                new IntRange(DefaultMinStoreys, DefaultMaxStoreys))
        {
        }

        public RowHouseSettings(int rowDepthCells, IntRange frontageCells, IntRange storeys)
        {
            if (rowDepthCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowDepthCells), rowDepthCells, "Row depth must be positive.");
            }

            if (frontageCells.Min <= 0 || frontageCells.Max < (frontageCells.Min * SplitHalves) - 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(frontageCells),
                    frontageCells,
                    "Frontage must be positive, and its max at least twice its min minus one so every street can be split.");
            }

            if (storeys.Min <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storeys), storeys, "Storeys must be positive.");
            }

            RowDepthCells = rowDepthCells;
            FrontageCells = frontageCells;
            Storeys = storeys;
        }

        /// <summary>How deep each house reaches from the street into the block, in cells.</summary>
        public int RowDepthCells { get; }

        /// <summary>How wide each house is along its street, in cells.</summary>
        public IntRange FrontageCells { get; }

        public IntRange Storeys { get; }

        /// <summary>Extra storeys for the corner houses, so the block's corners stand out like in the concept art.</summary>
        public int CornerExtraStoreys { get; private set; } = DefaultCornerExtraStoreys;

        public RowHouseSettings WithCornerExtraStoreys(int extraStoreys)
        {
            if (extraStoreys < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(extraStoreys), extraStoreys, "Must not be negative.");
            }

            CornerExtraStoreys = extraStoreys;
            return this;
        }
    }
}
