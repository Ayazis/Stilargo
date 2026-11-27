using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Builds a deterministic town on a grid of streets (including a ring road on the border). The
    /// <see cref="TownPlan"/> puts the open market square in the middle, house blocks with courtyard gardens around
    /// it and parks further out. The <see cref="StreetShaper"/> merges and splits outskirts blocks so the streets
    /// there are less regular, and the <see cref="NeighbourhoodPicker"/> turns each outskirts block into a plaza,
    /// old town, villas or workshops.
    /// </summary>
    public sealed class CityGenerator
    {
        private readonly int _seed;
        private readonly CityConfig _config;
        private readonly TownSettings _town;
        private readonly HouseBlockLayout _houseBlocks;
        private readonly Dictionary<BlockRole, IBlockLayout> _layouts;

        public CityGenerator(int seed, CityConfig config)
            : this(seed, config, new TownSettings())
        {
        }

        public CityGenerator(int seed, CityConfig config, TownSettings town)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Validate();
            _seed = seed;
            _config = config;
            _town = town ?? throw new ArgumentNullException(nameof(town));
            _houseBlocks = new HouseBlockLayout(town.HouseBlock);
            _layouts = CreateLayouts();
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
            List<TownBlock> blocks = CreateBlocks(period, random);
            blocks = new StreetShaper(_town.Streets, _config.BlocksX, _config.MinPlotSize).Shape(blocks, grid, random);
            blocks = NeighbourhoodPicker.Pick(blocks, _town.Neighbourhoods, random);

            List<Plot> plots = new List<Plot>();
            foreach (TownBlock block in blocks)
            {
                LayoutFor(block).Lay(block.Area, random, plots);
            }

            return new CityLayout(grid, blocks.Select(block => block.Area).ToList(), plots);
        }

        private Dictionary<BlockRole, IBlockLayout> CreateLayouts()
        {
            var splitter = new PlotSplitter(new IntRange(_config.MinPlotSize, _config.MaxPlotSize));
            NeighbourhoodSettings neighbourhoods = _town.Neighbourhoods;
            return new Dictionary<BlockRole, IBlockLayout>
            {
                { BlockRole.MarketSquare, new OpenBlockLayout(PlotUse.Market) },
                { BlockRole.Plaza, new OpenBlockLayout(PlotUse.Plaza) },
                { BlockRole.Park, new OpenBlockLayout(PlotUse.Park) },
                { BlockRole.HouseBlock, _houseBlocks },
                {
                    BlockRole.OldTown,
                    new MixedBlockLayout(splitter, new IntRange(_config.MinStoreys, _config.MaxStoreys), OpenGroundShare.None)
                },
                {
                    BlockRole.Villas,
                    new MixedBlockLayout(
                        splitter,
                        new IntRange(neighbourhoods.VillaStoreys, neighbourhoods.VillaStoreys),
                        new OpenGroundShare(PlotUse.Garden, neighbourhoods.VillaGardenShare))
                },
                {
                    BlockRole.Workshops,
                    new MixedBlockLayout(
                        splitter,
                        new IntRange(neighbourhoods.WorkshopStoreys, neighbourhoods.WorkshopStoreys),
                        new OpenGroundShare(PlotUse.Yard, neighbourhoods.WorkshopYardShare))
                },
            };
        }

        // A house block too small for two rows and a courtyard gets old-town houses instead.
        private IBlockLayout LayoutFor(TownBlock block)
        {
            if (block.Role == BlockRole.HouseBlock && !_houseBlocks.Fits(block.Area))
            {
                return _layouts[BlockRole.OldTown];
            }

            if (!_layouts.TryGetValue(block.Role, out IBlockLayout layout))
            {
                throw new InvalidOperationException($"No layout for {block.Role} blocks.");
            }

            return layout;
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

        private List<TownBlock> CreateBlocks(int period, Random random)
        {
            BlockRole[] roles = TownPlan.Assign(_config.BlocksX, _config.BlocksY, _town.ParkBlockCount, random);
            List<TownBlock> blocks = new List<TownBlock>();
            for (int blockY = 0; blockY < _config.BlocksY; blockY++)
            {
                for (int blockX = 0; blockX < _config.BlocksX; blockX++)
                {
                    GridPosition min = new GridPosition(
                        _config.RoadWidth + (blockX * period),
                        _config.RoadWidth + (blockY * period));
                    var area = new GridRect(min, _config.BlockSize, _config.BlockSize);
                    blocks.Add(new TownBlock(area, roles[(blockY * _config.BlocksX) + blockX]));
                }
            }

            return blocks;
        }
    }
}
