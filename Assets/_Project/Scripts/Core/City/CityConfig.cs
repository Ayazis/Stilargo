using System;

namespace Stilargo.Core.City
{
    /// <summary>Tunable parameters for <see cref="CityGenerator"/>. <see cref="Validate"/> throws on invalid values.</summary>
    public sealed class CityConfig
    {
        private const int DefaultBlocksX = 4;
        private const int DefaultBlocksY = 4;
        private const int DefaultBlockSize = 12;
        private const int DefaultRoadWidth = 2;
        private const int DefaultMinPlotSize = 3;
        private const int DefaultMaxPlotSize = 6;
        private const int DefaultMinStoreys = 1;
        private const int DefaultMaxStoreys = 4;
        private const float DefaultCellSize = 2f;
        private const int SplitHalves = 2;

        public int BlocksX { get; set; } = DefaultBlocksX;

        public int BlocksY { get; set; } = DefaultBlocksY;

        /// <summary>Edge length of a square block, in cells.</summary>
        public int BlockSize { get; set; } = DefaultBlockSize;

        /// <summary>Road width, in cells.</summary>
        public int RoadWidth { get; set; } = DefaultRoadWidth;

        /// <summary>Smallest plot edge, in cells.</summary>
        public int MinPlotSize { get; set; } = DefaultMinPlotSize;

        /// <summary>Largest plot edge, in cells. At least twice MinPlotSize so any oversized area can be split.</summary>
        public int MaxPlotSize { get; set; } = DefaultMaxPlotSize;

        public int MinStoreys { get; set; } = DefaultMinStoreys;

        public int MaxStoreys { get; set; } = DefaultMaxStoreys;

        /// <summary>World size of one cell edge, in metres.</summary>
        public float CellSize { get; set; } = DefaultCellSize;

        public void Validate()
        {
            RequirePositive(BlocksX, nameof(BlocksX));
            RequirePositive(BlocksY, nameof(BlocksY));
            RequirePositive(BlockSize, nameof(BlockSize));
            RequirePositive(RoadWidth, nameof(RoadWidth));
            RequirePositive(MinPlotSize, nameof(MinPlotSize));
            RequirePositive(MinStoreys, nameof(MinStoreys));

            if (CellSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(CellSize), CellSize, "Must be positive.");
            }

            if (MaxPlotSize < MinPlotSize * SplitHalves)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxPlotSize), MaxPlotSize, "Must be at least twice MinPlotSize.");
            }

            if (BlockSize < MinPlotSize)
            {
                throw new ArgumentOutOfRangeException(nameof(BlockSize), BlockSize, "Must be at least MinPlotSize.");
            }

            if (MaxStoreys < MinStoreys)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxStoreys), MaxStoreys, "Must be at least MinStoreys.");
            }
        }

        private static void RequirePositive(int value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be positive.");
            }
        }
    }
}
