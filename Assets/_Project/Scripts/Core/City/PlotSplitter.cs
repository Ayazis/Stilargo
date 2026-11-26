using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Recursively cuts an area into plots whose edges lie in the size range. The range's max must be at least
    /// twice its min, so any edge above the max always has a valid split point.
    /// </summary>
    public sealed class PlotSplitter
    {
        private const int SplitHalves = 2;

        private readonly IntRange _plotSize;

        public PlotSplitter(IntRange plotSize)
        {
            if (plotSize.Min <= 0 || plotSize.Max < plotSize.Min * SplitHalves)
            {
                throw new ArgumentOutOfRangeException(nameof(plotSize), plotSize, "Max must be at least twice a positive min.");
            }

            _plotSize = plotSize;
        }

        public void Split(GridRect area, Random random, ICollection<GridRect> plots)
        {
            if (area.Width > _plotSize.Max)
            {
                int split = NextSplit(area.Width, random);
                Split(new GridRect(area.Min, split, area.Height), random, plots);
                Split(new GridRect(new GridPosition(area.Min.X + split, area.Min.Y), area.Width - split, area.Height), random, plots);
                return;
            }

            if (area.Height > _plotSize.Max)
            {
                int split = NextSplit(area.Height, random);
                Split(new GridRect(area.Min, area.Width, split), random, plots);
                Split(new GridRect(new GridPosition(area.Min.X, area.Min.Y + split), area.Width, area.Height - split), random, plots);
                return;
            }

            plots.Add(area);
        }

        private int NextSplit(int length, Random random)
        {
            return random.Next(_plotSize.Min, length - _plotSize.Min + 1);
        }
    }
}
