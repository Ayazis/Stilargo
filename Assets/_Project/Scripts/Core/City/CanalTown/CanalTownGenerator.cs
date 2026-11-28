using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>
    /// Builds a town with character: countryside with farms, fields and orchards around a ring road, a canal with quays and bridges across the south,
    /// staggered streets, a main square with a fountain, a church square, a market, parks inside the town, a boatyard
    /// on the canal and house blocks around courtyard gardens. The player's soldiers enter at the west map edge, the
    /// rival's at the east edge, each on a high road. Same seed, same town.
    /// </summary>
    public sealed class CanalTownGenerator
    {
        private readonly int _seed;
        private readonly CanalTownSettings _settings;

        public CanalTownGenerator(int seed, CanalTownSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _settings.Validate();
            _seed = seed;
        }

        public CanalTown Generate()
        {
            var random = new Random(_seed);
            var grid = new CityGrid(_settings.WidthCells, _settings.HeightCells, _settings.CellSizeMetres);
            var plots = new List<Plot>();

            GridRect town = Shrink(RectCarving.Rect(0, 0, grid.Width, grid.Height), _settings.CountrysideCells);
            MarkRing(grid, town);
            GridRect inner = Shrink(town, _settings.StreetWidthCells);

            Canal canal = LayCanal(grid, inner, random, plots);
            int highRoadY = HighRoadY(inner);
            GridRect westRoad = RectCarving.Rect(0, highRoadY, town.Min.X, _settings.HighRoadWidthCells);
            GridRect eastRoad = RectCarving.Rect(town.XMax, highRoadY, grid.Width - town.XMax, _settings.HighRoadWidthCells);
            new Countryside(_settings, random).Lay(grid, town, canal.Water, new[] { westRoad, eastRoad }, plots);
            List<List<GridRect>> bands = LayBands(grid, inner, canal, random);
            List<GridRect> blocks = bands.SelectMany(band => band).ToList();
            Dictionary<GridRect, TownBlockKind> kinds = new BlockPlanner(_settings, random).Assign(bands, inner);

            var furnisher = new BlockFurnisher(_settings, random);
            foreach (GridRect block in blocks)
            {
                furnisher.Furnish(block, kinds[block], plots);
            }

            int entryY = highRoadY + (_settings.HighRoadWidthCells / 2);
            var playerEntry = new GridPosition(0, entryY);
            var rivalEntry = new GridPosition(grid.Width - 1, entryY);
            CityLayout layout = HouseStyler.Style(new CityLayout(grid, blocks, plots));
            return new CanalTown(layout, canal, (playerEntry, rivalEntry));
        }

        // High roads run from the west and east map edges to the ring road, halfway up the town north of the canal.
        private int HighRoadY(GridRect inner)
        {
            int canalTop = inner.Min.Y + (int)(inner.Height * _settings.CanalPositionShare) + _settings.CanalWidthCells;
            return (canalTop + inner.YMax - _settings.HighRoadWidthCells) / 2;
        }

        private void MarkRing(CityGrid grid, GridRect town)
        {
            GridRect inner = Shrink(town, _settings.StreetWidthCells);
            foreach (GridRect side in RectCarving.Around(town, inner))
            {
                MarkRoad(grid, side);
            }
        }

        // Quay, water, quay across the whole town, with bridges carrying streets over the water.
        private Canal LayCanal(CityGrid grid, GridRect inner, Random random, List<Plot> plots)
        {
            int street = _settings.StreetWidthCells;
            int waterY = inner.Min.Y + (int)(inner.Height * _settings.CanalPositionShare);
            GridRect southQuay = RectCarving.Rect(inner.Min.X, waterY - street, inner.Width, street);
            GridRect water = RectCarving.Rect(inner.Min.X - street, waterY, inner.Width + (street * 2), _settings.CanalWidthCells);
            GridRect northQuay = RectCarving.Rect(inner.Min.X, water.YMax, inner.Width, street);
            MarkRoad(grid, southQuay);
            MarkRoad(grid, northQuay);

            List<int> bridges = PickBridges(inner, random);
            int waterStart = water.Min.X;
            foreach (int bridgeX in bridges)
            {
                plots.Add(new Plot(RectCarving.Rect(waterStart, water.Min.Y, bridgeX - waterStart, water.Height), 1, PlotUse.Water));
                MarkRoad(grid, RectCarving.Rect(bridgeX, water.Min.Y, street, water.Height));
                waterStart = bridgeX + street;
            }

            plots.Add(new Plot(RectCarving.Rect(waterStart, water.Min.Y, water.XMax - waterStart, water.Height), 1, PlotUse.Water));
            ClearRingOverWater(grid, water, inner);
            return new Canal(water, southQuay.Min.Y, northQuay.YMax, bridges, street);
        }

        // The canal flows out of town under the ring road, so the ring has no road where the water passes.
        private static void ClearRingOverWater(CityGrid grid, GridRect water, GridRect inner)
        {
            foreach (GridPosition cell in water.Cells().Where(cell => cell.X < inner.Min.X || cell.X >= inner.XMax))
            {
                grid.ClearRoad(cell);
            }
        }

        private List<int> PickBridges(GridRect inner, Random random)
        {
            int spacing = inner.Width / (_settings.BridgeCount + 1);
            var bridges = new List<int>();
            for (int index = 1; index <= _settings.BridgeCount; index++)
            {
                int jitter = random.Next(-spacing / 4, (spacing / 4) + 1);
                bridges.Add(inner.Min.X + (spacing * index) + jitter);
            }

            return bridges;
        }

        // Horizontal bands of blocks; each band has its own cross streets, so streets stagger like an old town's.
        private List<List<GridRect>> LayBands(CityGrid grid, GridRect inner, Canal canal, Random random)
        {
            var bands = new List<List<GridRect>>();
            bands.Add(LayBand(grid, inner, inner.Min.Y, canal.SouthEdgeY, canal.Bridges, random));

            List<(int Start, int End)> northRows = Split(canal.NorthEdgeY, inner.YMax, random);
            for (int index = 0; index < northRows.Count; index++)
            {
                (int start, int end) = northRows[index];
                if (index > 0)
                {
                    MarkRoad(grid, RectCarving.Rect(inner.Min.X, start - _settings.StreetWidthCells, inner.Width, _settings.StreetWidthCells));
                }

                // Only the band on the quay must line up with the bridges.
                IReadOnlyList<int> forced = index == 0 ? canal.Bridges : Array.Empty<int>();
                bands.Add(LayBand(grid, inner, start, end, forced, random));
            }

            return bands;
        }

        private List<GridRect> LayBand(CityGrid grid, GridRect inner, int startY, int endY, IReadOnlyList<int> forcedStreets, Random random)
        {
            var blocks = new List<GridRect>();
            int street = _settings.StreetWidthCells;
            int from = inner.Min.X;
            foreach (int streetX in forcedStreets.Concat(new[] { inner.XMax }))
            {
                foreach ((int start, int end) in Split(from, streetX, random))
                {
                    blocks.Add(RectCarving.Rect(start, startY, end - start, endY - startY));
                    if (end < inner.XMax)
                    {
                        MarkRoad(grid, RectCarving.Rect(end, startY, street, endY - startY));
                    }
                }

                from = streetX + street;
            }

            return blocks;
        }

        // Splits [start, end) into blocks between streets of random but bounded size; the last block takes the rest.
        private List<(int Start, int End)> Split(int start, int end, Random random)
        {
            var parts = new List<(int, int)>();
            int street = _settings.StreetWidthCells;
            int position = start;
            while (end - position > _settings.MaxBlockCells)
            {
                int size = random.Next(_settings.MinBlockCells, _settings.MaxBlockCells + 1);
                if (end - (position + size + street) < _settings.MinBlockCells)
                {
                    size = (end - position - street) / 2;
                }

                parts.Add((position, position + size));
                position += size + street;
            }

            parts.Add((position, end));
            return parts;
        }

        private static GridRect Shrink(GridRect rect, int cells)
        {
            return RectCarving.Rect(rect.Min.X + cells, rect.Min.Y + cells, rect.Width - (cells * 2), rect.Height - (cells * 2));
        }

        private static void MarkRoad(CityGrid grid, GridRect area)
        {
            foreach (GridPosition cell in area.Cells())
            {
                grid.MarkRoad(cell);
            }
        }
    }
}
