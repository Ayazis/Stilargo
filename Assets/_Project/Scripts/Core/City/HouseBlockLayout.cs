using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Lays out a perimeter block: a row of town houses along each of its four streets around a courtyard garden.
    /// The south and north rows run the full width, so their end houses are the block's corners and rise higher;
    /// the west and east rows fill the gap between them.
    /// </summary>
    public sealed class HouseBlockLayout : IBlockLayout
    {
        private const int SplitHalves = 2;
        private const int CourtyardStoreys = 1;

        private readonly RowHouseSettings _settings;

        public HouseBlockLayout(RowHouseSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>True when the block is big enough for two rows and a courtyard at least one cell wide.</summary>
        public bool Fits(GridRect block)
        {
            int rows = _settings.RowDepthCells * SplitHalves;
            return block.Width > rows && block.Height > rows;
        }

        public void Lay(GridRect block, Random random, ICollection<Plot> plots)
        {
            if (!Fits(block))
            {
                throw new ArgumentException($"Block {block} is too small for rows {_settings.RowDepthCells} cells deep.", nameof(block));
            }

            int depth = _settings.RowDepthCells;
            int sideLength = block.Height - (depth * SplitHalves);
            int sideSouth = block.Min.Y + depth;
            LayRow(new GridRect(block.Min, block.Width, depth), random, plots);
            LayRow(new GridRect(new GridPosition(block.Min.X, block.YMax - depth), block.Width, depth), random, plots);
            LayRow(new GridRect(new GridPosition(block.Min.X, sideSouth), depth, sideLength), random, plots);
            LayRow(new GridRect(new GridPosition(block.XMax - depth, sideSouth), depth, sideLength), random, plots);

            var courtyard = new GridRect(new GridPosition(block.Min.X + depth, sideSouth), block.Width - (depth * SplitHalves), sideLength);
            plots.Add(new Plot(courtyard, CourtyardStoreys, PlotUse.Garden));
        }

        // Cuts a row into houses along its street. The south and north rows are wider than one house is deep, the
        // west and east rows exactly as wide. A row shorter than the smallest frontage is one house.
        private void LayRow(GridRect row, Random random, ICollection<Plot> plots)
        {
            bool runsAlongX = row.Width > _settings.RowDepthCells;
            int length = runsAlongX ? row.Width : row.Height;
            int offset = 0;
            while (offset < length)
            {
                int frontage = NextFrontage(length - offset, random);
                GridRect area = runsAlongX
                    ? new GridRect(new GridPosition(row.Min.X + offset, row.Min.Y), frontage, row.Height)
                    : new GridRect(new GridPosition(row.Min.X, row.Min.Y + offset), row.Width, frontage);
                bool isCorner = runsAlongX && (offset == 0 || offset + frontage == length);
                int storeys = _settings.Storeys.Pick(random) + (isCorner ? _settings.CornerExtraStoreys : 0);
                plots.Add(new Plot(area, storeys));
                offset += frontage;
            }
        }

        // The settings guarantee max >= 2 * min - 1, so what is left after a cut is never below the minimum.
        private int NextFrontage(int remaining, Random random)
        {
            IntRange frontage = _settings.FrontageCells;
            if (remaining <= frontage.Max)
            {
                return remaining;
            }

            return random.Next(frontage.Min, Math.Min(frontage.Max, remaining - frontage.Min) + 1);
        }
    }
}
