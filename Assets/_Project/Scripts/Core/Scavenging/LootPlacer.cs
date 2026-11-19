using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Scavenging
{
    /// <summary>
    /// Deterministically places loot caches on building doorsteps: the road cell just outside the middle of a
    /// plot side that faces a street. Units can't enter buildings yet, so the doorstep is the closest
    /// reachable spot to "loot inside a building". At most one cache per building.
    /// </summary>
    public sealed class LootPlacer
    {
        private const float HalfCell = 0.5f;
        private const int Halves = 2;

        private readonly int _seed;
        private readonly LootConfig _config;

        public LootPlacer(int seed, LootConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Validate();
            _seed = seed;
            _config = config;
        }

        public IReadOnlyList<LootCache> Place(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            Random random = new Random(_seed);
            List<GridPosition> doorsteps = FindDoorsteps(layout);
            Shuffle(doorsteps, random);

            int count = Math.Min(_config.CacheCount, doorsteps.Count);
            List<LootCache> caches = new List<LootCache>(count);
            for (int i = 0; i < count; i++)
            {
                int value = random.Next(_config.MinValue, _config.MaxValue + 1);
                caches.Add(new LootCache(ToWorld(doorsteps[i], layout.CellSize), value));
            }

            return caches;
        }

        private static List<GridPosition> FindDoorsteps(CityLayout layout)
        {
            List<GridPosition> doorsteps = new List<GridPosition>();
            foreach (Plot plot in layout.Plots)
            {
                foreach (GridPosition candidate in SideMidpointsOutside(plot.Area))
                {
                    if (layout.IsRoad(candidate))
                    {
                        doorsteps.Add(candidate);
                        break;
                    }
                }
            }

            return doorsteps;
        }

        private static IEnumerable<GridPosition> SideMidpointsOutside(GridRect area)
        {
            int midX = area.Min.X + (area.Width / Halves);
            int midY = area.Min.Y + (area.Height / Halves);
            yield return new GridPosition(midX, area.Min.Y - 1);
            yield return new GridPosition(midX, area.YMax);
            yield return new GridPosition(area.Min.X - 1, midY);
            yield return new GridPosition(area.XMax, midY);
        }

        // Fisher-Yates driven by the seeded Random, so placement stays deterministic per seed.
        private static void Shuffle(List<GridPosition> items, Random random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (items[i], items[j]) = (items[j], items[i]);
            }
        }

        private static WorldPoint ToWorld(GridPosition cell, float cellSize)
        {
            return new WorldPoint((cell.X + HalfCell) * cellSize, (cell.Y + HalfCell) * cellSize);
        }
    }
}
