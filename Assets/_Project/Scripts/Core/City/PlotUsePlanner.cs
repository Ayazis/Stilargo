using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Hands house plots of a generated city to its landmarks. The same seed always gives the same city. Landmarks
    /// that want the largest plots, or the largest facing the market square, get them in rule order; the others are
    /// scattered. Uses never share a block while a block without one is left, so the landmarks spread over the city.
    /// Plots the town plan already gave a rule's use count towards that rule.
    /// </summary>
    public sealed class PlotUsePlanner
    {
        private readonly DistrictSettings _settings;

        public PlotUsePlanner(DistrictSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public CityLayout Plan(CityLayout layout, int seed)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            var random = new Random(seed);
            var plots = layout.Plots.ToList();
            var taken = new HashSet<int>();
            var usedBlocks = new HashSet<int>();

            foreach (PlotUseRule rule in _settings.Rules)
            {
                int alreadyPlaced = ApplyToExisting(rule, plots, taken);
                for (int placed = alreadyPlaced; placed < rule.Count; placed++)
                {
                    int index = PickPlot(rule.Placement, layout, taken, usedBlocks, random);
                    if (index < 0)
                    {
                        break;
                    }

                    taken.Add(index);
                    usedBlocks.Add(BlockOf(layout, plots[index]));
                    plots[index] = plots[index].UsedFor(rule);
                }
            }

            return layout.WithPlots(plots);
        }

        // A town plan may already have laid out some of a rule's plots (a church on its square, a canal); those get
        // the rule's height and income and count towards its total.
        private static int ApplyToExisting(PlotUseRule rule, List<Plot> plots, ISet<int> taken)
        {
            int count = 0;
            for (int index = 0; index < plots.Count; index++)
            {
                if (plots[index].Use == rule.Use)
                {
                    plots[index] = plots[index].UsedFor(rule);
                    taken.Add(index);
                    count++;
                }
            }

            return count;
        }

        private static int PickPlot(PlotPlacement placement, CityLayout layout, ISet<int> taken, ISet<int> usedBlocks, Random random)
        {
            List<int> free = Enumerable.Range(0, layout.Plots.Count)
                .Where(index => !taken.Contains(index) && layout.Plots[index].Use == PlotUse.House)
                .ToList();
            if (free.Count == 0)
            {
                return -1;
            }

            List<int> inFreshBlocks = free.Where(index => !usedBlocks.Contains(BlockOf(layout, layout.Plots[index]))).ToList();
            List<int> candidates = inFreshBlocks.Count > 0 ? inFreshBlocks : free;

            if (placement == PlotPlacement.FacingSquare)
            {
                return LargestOf(FacingSquare(layout, candidates), layout);
            }

            if (placement == PlotPlacement.Largest)
            {
                return LargestOf(candidates, layout);
            }

            return candidates[random.Next(candidates.Count)];
        }

        private static int LargestOf(List<int> candidates, CityLayout layout)
        {
            return candidates
                .OrderByDescending(index => CellCount(layout.Plots[index]))
                .ThenBy(index => index)
                .First();
        }

        // The front row is the candidates nearest the square, the street gaps along both axes added up: a plot on
        // a diagonal block only meets the square at a corner, so it counts as further away. Without a square,
        // every candidate counts.
        private static List<int> FacingSquare(CityLayout layout, List<int> candidates)
        {
            List<GridRect> squares = layout.Plots.Where(plot => plot.Use == PlotUse.Market).Select(plot => plot.Area).ToList();
            if (squares.Count == 0)
            {
                return candidates;
            }

            Dictionary<int, int> gaps = candidates.ToDictionary(
                index => index,
                index => squares.Min(square => Gap(square, layout.Plots[index].Area)));
            int nearest = gaps.Values.Min();
            return candidates.Where(index => gaps[index] == nearest).ToList();
        }

        private static int Gap(GridRect first, GridRect second)
        {
            int gapX = Math.Max(0, Math.Max(first.Min.X - second.XMax, second.Min.X - first.XMax));
            int gapY = Math.Max(0, Math.Max(first.Min.Y - second.YMax, second.Min.Y - first.YMax));
            return gapX + gapY;
        }

        private static int CellCount(Plot plot)
        {
            return plot.Area.Width * plot.Area.Height;
        }

        private static int BlockOf(CityLayout layout, Plot plot)
        {
            for (int index = 0; index < layout.Blocks.Count; index++)
            {
                if (layout.Blocks[index].Overlaps(plot.Area))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
