using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>
    /// Decides what each block becomes. Bands run south to north: band 0 lies south of the canal, band 1 on the
    /// north quay. The main square sits nearest the town centre, church and market next to it, a park on the quay,
    /// a boatyard south of the canal, other parks spread over the rest, free-standing homes on part of the town's
    /// edge and row houses everywhere else.
    /// </summary>
    internal sealed class BlockPlanner
    {
        private const int SouthBand = 0;
        private const int QuayBand = 1;

        private readonly CanalTownSettings _settings;
        private readonly Random _random;

        public BlockPlanner(CanalTownSettings settings, Random random)
        {
            _settings = settings;
            _random = random;
        }

        public Dictionary<GridRect, TownBlockKind> Assign(List<List<GridRect>> bands, GridRect town)
        {
            Dictionary<GridRect, TownBlockKind> kinds = bands.SelectMany(band => band).ToDictionary(block => block, _ => TownBlockKind.Houses);
            List<GridRect> north = bands.Skip(QuayBand).SelectMany(band => band).ToList();
            var centre = (X: town.Min.X + (town.Width / 2f), Y: town.Min.Y + (town.Height / 2f));

            GridRect main = north.OrderBy(block => Distance(block, centre)).First();
            kinds[main] = TownBlockKind.MainSquare;
            AssignNearest(kinds, north, main, TownBlockKind.ChurchSquare);
            AssignNearest(kinds, north, main, TownBlockKind.Market);
            AssignRandom(kinds, bands[QuayBand], TownBlockKind.CanalPark);
            AssignRandom(kinds, bands[SouthBand], TownBlockKind.Boatyard);
            for (int park = 0; park < _settings.ParkCount; park++)
            {
                AssignRandom(kinds, Inland(north, town), TownBlockKind.Park);
            }

            foreach (GridRect block in Free(kinds, kinds.Keys.ToList()).Where(block => !IsInland(block, town)).ToList())
            {
                if (_random.NextDouble() < _settings.DetachedEdgeShare)
                {
                    kinds[block] = TownBlockKind.Detached;
                }
            }

            return kinds;
        }

        private static void AssignNearest(Dictionary<GridRect, TownBlockKind> kinds, List<GridRect> candidates, GridRect to, TownBlockKind kind)
        {
            var target = (X: to.Min.X + (to.Width / 2f), Y: to.Min.Y + (to.Height / 2f));
            GridRect block = Free(kinds, candidates).OrderBy(candidate => Distance(candidate, target)).FirstOrDefault();
            if (Free(kinds, candidates).Any())
            {
                kinds[block] = kind;
            }
        }

        private void AssignRandom(Dictionary<GridRect, TownBlockKind> kinds, List<GridRect> candidates, TownBlockKind kind)
        {
            List<GridRect> free = Free(kinds, candidates).ToList();
            if (free.Count > 0)
            {
                kinds[free[_random.Next(free.Count)]] = kind;
            }
        }

        // Parks belong inside the town, not on its edge, so prefer blocks that don't touch the ring road.
        private static List<GridRect> Inland(List<GridRect> blocks, GridRect town)
        {
            List<GridRect> inland = blocks.Where(block => IsInland(block, town)).ToList();
            return inland.Count > 0 ? inland : blocks;
        }

        private static bool IsInland(GridRect block, GridRect town)
        {
            return block.Min.X > town.Min.X && block.XMax < town.XMax && block.Min.Y > town.Min.Y && block.YMax < town.YMax;
        }

        private static IEnumerable<GridRect> Free(Dictionary<GridRect, TownBlockKind> kinds, List<GridRect> candidates)
        {
            return candidates.Where(block => kinds[block] == TownBlockKind.Houses);
        }

        private static float Distance(GridRect block, (float X, float Y) point)
        {
            float dx = block.Min.X + (block.Width / 2f) - point.X;
            float dy = block.Min.Y + (block.Height / 2f) - point.Y;
            return (dx * dx) + (dy * dy);
        }
    }
}
