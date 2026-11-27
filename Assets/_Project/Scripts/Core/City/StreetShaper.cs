using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Breaks up the regular grid in the outskirts: a few pairs of neighbouring blocks lose the street between them
    /// and become one long block, and some blocks are cut in two by a narrow alley. Streets next to the market
    /// square and the house blocks never change, so places on them (home, the hill) stay on a road.
    /// </summary>
    public sealed class StreetShaper
    {
        private const int SplitHalves = 2;

        private readonly StreetSettings _settings;
        private readonly int _blocksX;
        private readonly int _minPlotSize;

        public StreetShaper(StreetSettings settings, int blocksX, int minPlotSize)
        {
            _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Validate();
            if (blocksX <= 0 || minPlotSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksX), "Block count and plot size must be positive.");
            }

            _blocksX = blocksX;
            _minPlotSize = minPlotSize;
        }

        /// <summary>Takes the blocks in grid order (row by row from the south-west) and edits the grid's roads to match.</summary>
        public List<TownBlock> Shape(IReadOnlyList<TownBlock> gridBlocks, CityGrid grid, Random random)
        {
            if (gridBlocks == null || grid == null || random == null)
            {
                throw new ArgumentNullException(gridBlocks == null ? nameof(gridBlocks) : grid == null ? nameof(grid) : nameof(random));
            }

            List<TownBlock> merged = Merge(gridBlocks, grid, random);
            var shaped = new List<TownBlock>();
            foreach (TownBlock block in merged)
            {
                AddWithAlley(block, grid, random, shaped);
            }

            return shaped;
        }

        private List<TownBlock> Merge(IReadOnlyList<TownBlock> gridBlocks, CityGrid grid, Random random)
        {
            List<(int First, int Second)> pairs = NeighbouringOutskirts(gridBlocks).OrderBy(_ => random.Next()).ToList();
            var paired = new HashSet<int>();
            var dropped = new HashSet<int>();
            var result = gridBlocks.ToList();
            foreach ((int first, int second) in pairs)
            {
                if (dropped.Count >= _settings.MergedBlockPairs)
                {
                    break;
                }

                if (paired.Contains(first) || paired.Contains(second))
                {
                    continue;
                }

                // The first block of the pair takes the union; the second is dropped.
                GridRect union = Union(gridBlocks[first].Area, gridBlocks[second].Area);
                ClearRoadsInside(union, grid);
                result[first] = new TownBlock(union, BlockRole.Outskirts);
                paired.Add(first);
                paired.Add(second);
                dropped.Add(second);
            }

            return result.Where((_, index) => !dropped.Contains(index)).ToList();
        }

        private IEnumerable<(int First, int Second)> NeighbouringOutskirts(IReadOnlyList<TownBlock> blocks)
        {
            for (int index = 0; index < blocks.Count; index++)
            {
                if (blocks[index].Role != BlockRole.Outskirts)
                {
                    continue;
                }

                int east = index + 1;
                if ((index % _blocksX) + 1 < _blocksX && blocks[east].Role == BlockRole.Outskirts)
                {
                    yield return (index, east);
                }

                int north = index + _blocksX;
                if (north < blocks.Count && blocks[north].Role == BlockRole.Outskirts)
                {
                    yield return (index, north);
                }
            }
        }

        private static GridRect Union(GridRect first, GridRect second)
        {
            int minX = Math.Min(first.Min.X, second.Min.X);
            int minY = Math.Min(first.Min.Y, second.Min.Y);
            int maxX = Math.Max(first.XMax, second.XMax);
            int maxY = Math.Max(first.YMax, second.YMax);
            return new GridRect(new GridPosition(minX, minY), maxX - minX, maxY - minY);
        }

        private static void ClearRoadsInside(GridRect area, CityGrid grid)
        {
            foreach (GridPosition cell in area.Cells())
            {
                grid.ClearRoad(cell);
            }
        }

        private void AddWithAlley(TownBlock block, CityGrid grid, Random random, List<TownBlock> shaped)
        {
            GridRect area = block.Area;
            bool cutsAcrossX = area.Width >= area.Height;
            int length = cutsAcrossX ? area.Width : area.Height;
            int width = _settings.AlleyWidthCells;
            bool canCut = length >= (_minPlotSize * SplitHalves) + width;
            if (block.Role != BlockRole.Outskirts || !canCut || random.NextDouble() >= _settings.AlleyChance)
            {
                shaped.Add(block);
                return;
            }

            int offset = random.Next(_minPlotSize, length - _minPlotSize - width + 1);
            GridRect alley = cutsAcrossX
                ? new GridRect(new GridPosition(area.Min.X + offset, area.Min.Y), width, area.Height)
                : new GridRect(new GridPosition(area.Min.X, area.Min.Y + offset), area.Width, width);
            foreach (GridPosition cell in alley.Cells())
            {
                grid.MarkRoad(cell);
            }

            int rest = length - offset - width;
            GridRect before = cutsAcrossX
                ? new GridRect(area.Min, offset, area.Height)
                : new GridRect(area.Min, area.Width, offset);
            GridRect after = cutsAcrossX
                ? new GridRect(new GridPosition(alley.XMax, area.Min.Y), rest, area.Height)
                : new GridRect(new GridPosition(area.Min.X, alley.YMax), area.Width, rest);
            shaped.Add(new TownBlock(before, BlockRole.Outskirts));
            shaped.Add(new TownBlock(after, BlockRole.Outskirts));
        }
    }
}
