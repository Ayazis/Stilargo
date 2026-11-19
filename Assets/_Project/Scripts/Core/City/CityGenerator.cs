using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Builds a deterministic grid city: a lattice of roads (including a ring road on the border)
    /// around square blocks, each block recursively split into building plots.
    /// </summary>
    public sealed class CityGenerator
    {
        private readonly int _seed;
        private readonly CityConfig _config;

        public CityGenerator(int seed, CityConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Validate();
            _seed = seed;
            _config = config;
        }

        public CityLayout Generate()
        {
            Random random = new Random(_seed);
            int period = _config.BlockSize + _config.RoadWidth;
            CityGrid grid = new CityGrid(
                (_config.BlocksX * period) + _config.RoadWidth,
                (_config.BlocksY * period) + _config.RoadWidth,
                _config.CellSize);

            MarkRoads(grid, period);
            List<GridRect> blocks = CreateBlocks(period);
            List<Plot> plots = new List<Plot>();
            foreach (GridRect block in blocks)
            {
                Subdivide(block, random, plots);
            }

            return new CityLayout(grid, blocks, plots);
        }

        private void MarkRoads(CityGrid grid, int period)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    if (IsOnRoadLine(x, period) || IsOnRoadLine(y, period))
                    {
                        grid.MarkRoad(new GridPosition(x, y));
                    }
                }
            }
        }

        private bool IsOnRoadLine(int coordinate, int period)
        {
            return coordinate % period < _config.RoadWidth;
        }

        private List<GridRect> CreateBlocks(int period)
        {
            List<GridRect> blocks = new List<GridRect>();
            for (int blockY = 0; blockY < _config.BlocksY; blockY++)
            {
                for (int blockX = 0; blockX < _config.BlocksX; blockX++)
                {
                    GridPosition min = new GridPosition(
                        _config.RoadWidth + (blockX * period),
                        _config.RoadWidth + (blockY * period));
                    blocks.Add(new GridRect(min, _config.BlockSize, _config.BlockSize));
                }
            }

            return blocks;
        }

        private void Subdivide(GridRect area, Random random, List<Plot> plots)
        {
            if (area.Width > _config.MaxPlotSize)
            {
                int split = NextSplit(area.Width, random);
                GridPosition rightMin = new GridPosition(area.Min.X + split, area.Min.Y);
                Subdivide(new GridRect(area.Min, split, area.Height), random, plots);
                Subdivide(new GridRect(rightMin, area.Width - split, area.Height), random, plots);
                return;
            }

            if (area.Height > _config.MaxPlotSize)
            {
                int split = NextSplit(area.Height, random);
                GridPosition topMin = new GridPosition(area.Min.X, area.Min.Y + split);
                Subdivide(new GridRect(area.Min, area.Width, split), random, plots);
                Subdivide(new GridRect(topMin, area.Width, area.Height - split), random, plots);
                return;
            }

            int storeys = random.Next(_config.MinStoreys, _config.MaxStoreys + 1);
            plots.Add(new Plot(area, storeys));
        }

        // Both halves must stay at least MinPlotSize. Validate guarantees MaxPlotSize >= 2 * MinPlotSize,
        // so any length above MaxPlotSize always has a valid split point.
        private int NextSplit(int length, Random random)
        {
            return random.Next(_config.MinPlotSize, length - _config.MinPlotSize + 1);
        }
    }
}
