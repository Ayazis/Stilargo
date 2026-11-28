using System;
using System.Collections.Generic;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>Fills one block between streets with plots according to its <see cref="TownBlockKind"/>.</summary>
    internal sealed class BlockFurnisher
    {
        private const int FountainCells = 2;
        private const int WellCells = 1;
        private const int MinCourtyardForFountain = 4;
        private const int ChurchWidthCells = 6;
        private const int ChurchDepthCells = 9;
        private const int SquareFacingExtraStoreys = 1;
        private const int DetachedGardenCells = 3;
        private const int DetachedMinHouseCells = 2;

        private readonly CanalTownSettings _settings;
        private readonly Random _random;
        private readonly HouseRows _rows;
        private readonly Dictionary<TownBlockKind, Action<GridRect, List<Plot>>> _furnishings;

        public BlockFurnisher(CanalTownSettings settings, Random random)
        {
            _settings = settings;
            _random = random;
            _rows = new HouseRows(settings, random);
            _furnishings = new Dictionary<TownBlockKind, Action<GridRect, List<Plot>>>
            {
                { TownBlockKind.Houses, (block, plots) => HouseRing(block, 0, plots) },
                { TownBlockKind.MainSquare, MainSquare },
                { TownBlockKind.ChurchSquare, ChurchSquare },
                { TownBlockKind.Market, Market },
                { TownBlockKind.CanalPark, CanalPark },
                { TownBlockKind.Park, Park },
                { TownBlockKind.Boatyard, Boatyard },
                { TownBlockKind.Detached, Detached },
            };
        }

        public void Furnish(GridRect block, TownBlockKind kind, List<Plot> plots)
        {
            _furnishings[kind](block, plots);
        }

        // Houses around the edge of the block and a courtyard garden with a well in the middle.
        private void HouseRing(GridRect block, int extraStoreys, List<Plot> plots)
        {
            int depth = _settings.HouseDepthCells;
            if (block.Width < (depth * 2) + 2 || block.Height < (depth * 2) + 2)
            {
                _rows.Fill(block, block, extraStoreys, plots);
                return;
            }

            GridRect courtyard = RectCarving.Rect(block.Min.X + depth, block.Min.Y + depth, block.Width - (depth * 2), block.Height - (depth * 2));
            foreach (GridRect strip in RectCarving.Around(block, courtyard))
            {
                _rows.Fill(strip, block, extraStoreys, plots);
            }

            AddWithCentrePiece(courtyard, PlotUse.Garden, plots);
        }

        // An open square with a fountain, framed by tall houses on its north side.
        private void MainSquare(GridRect block, List<Plot> plots)
        {
            int depth = _settings.HouseDepthCells;
            GridRect houses = RectCarving.Rect(block.Min.X, block.YMax - depth, block.Width, depth);
            _rows.Fill(houses, block, SquareFacingExtraStoreys, plots);
            GridRect square = RectCarving.Rect(block.Min.X, block.Min.Y, block.Width, block.Height - depth);
            AddWithCentrePiece(square, PlotUse.Plaza, plots, FountainCells);
        }

        // The church at the back, its garden around it and a small square in front.
        private void ChurchSquare(GridRect block, List<Plot> plots)
        {
            int depth = Math.Min(ChurchDepthCells, block.Height - 3);
            GridRect back = RectCarving.Rect(block.Min.X, block.YMax - depth, block.Width, depth);
            GridRect church = RectCarving.Centred(back, Math.Min(ChurchWidthCells, block.Width - 2), depth);
            plots.Add(new Plot(church, 2, PlotUse.Church));
            foreach (GridRect garden in RectCarving.Around(back, church))
            {
                plots.Add(new Plot(garden, 1, PlotUse.Garden));
            }

            plots.Add(new Plot(RectCarving.Rect(block.Min.X, block.Min.Y, block.Width, block.Height - depth), 1, PlotUse.Plaza));
        }

        // Market stalls on an open square with houses along two sides.
        private void Market(GridRect block, List<Plot> plots)
        {
            int depth = _settings.HouseDepthCells;
            GridRect north = RectCarving.Rect(block.Min.X, block.YMax - depth, block.Width, depth);
            GridRect west = RectCarving.Rect(block.Min.X, block.Min.Y, depth, block.Height - depth);
            _rows.Fill(north, block, SquareFacingExtraStoreys, plots);
            _rows.Fill(west, block, SquareFacingExtraStoreys, plots);
            plots.Add(new Plot(RectCarving.Rect(block.Min.X + depth, block.Min.Y, block.Width - depth, block.Height - depth), 1, PlotUse.Market));
        }

        // A landscaped park with a pond, the one the quay walks past.
        private void CanalPark(GridRect block, List<Plot> plots)
        {
            GridRect pond = RectCarving.Centred(block, block.Width / 2, block.Height / 2);
            pond = RectCarving.Rect(pond.Min.X + _random.Next(-1, 2), pond.Min.Y, pond.Width, pond.Height);
            plots.Add(new Plot(pond, 1, PlotUse.Water));
            AddAround(block, pond, PlotUse.Park, plots);
        }

        private void Park(GridRect block, List<Plot> plots)
        {
            AddWithCentrePiece(block, PlotUse.Park, plots, FountainCells);
        }

        // Workshops along the street side and an open yard down to the canal.
        private void Boatyard(GridRect block, List<Plot> plots)
        {
            int depth = _settings.HouseDepthCells;
            GridRect workshops = RectCarving.Rect(block.Min.X, block.Min.Y, block.Width, depth);
            _rows.Fill(workshops, block, -1, plots);
            plots.Add(new Plot(RectCarving.Rect(block.Min.X, block.Min.Y + depth, block.Width, block.Height - depth), 1, PlotUse.Yard));
        }

        // Free-standing homes, each in its own garden, so neighbours don't touch.
        private void Detached(GridRect block, List<Plot> plots)
        {
            int columns = Math.Max(1, block.Width / _settings.DetachedLotCells);
            int rows = Math.Max(1, block.Height / _settings.DetachedLotCells);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    DetachedLot(LotOf(block, column, row, columns, rows), block, plots);
                }
            }
        }

        private static GridRect LotOf(GridRect block, int column, int row, int columns, int rows)
        {
            int x0 = block.Min.X + (block.Width * column / columns);
            int x1 = block.Min.X + (block.Width * (column + 1) / columns);
            int y0 = block.Min.Y + (block.Height * row / rows);
            int y1 = block.Min.Y + (block.Height * (row + 1) / rows);
            return RectCarving.Rect(x0, y0, x1 - x0, y1 - y0);
        }

        // The house stands against the street (so it can be entered and garrisoned) with its garden behind and beside it.
        private void DetachedLot(GridRect lot, GridRect block, List<Plot> plots)
        {
            int width = Math.Max(DetachedMinHouseCells, lot.Width - DetachedGardenCells - _random.Next(2));
            int depth = Math.Max(DetachedMinHouseCells, lot.Height - DetachedGardenCells - _random.Next(2));
            int x = AgainstStreet(lot.Min.X, lot.XMax, block.Min.X, block.XMax, width);
            int y = AgainstStreet(lot.Min.Y, lot.YMax, block.Min.Y, block.YMax, depth);
            GridRect house = RectCarving.Rect(x, y, width, depth);
            plots.Add(new Plot(house, _random.Next(_settings.MinStoreys - 1, _settings.MaxStoreys) + 1));
            AddAround(lot, house, PlotUse.Garden, plots);
        }

        // Along one axis: flush with the block edge the lot shares, otherwise a random spot inside the lot.
        private int AgainstStreet(int lotMin, int lotMax, int blockMin, int blockMax, int size)
        {
            if (lotMin == blockMin)
            {
                return lotMin;
            }

            if (lotMax == blockMax)
            {
                return lotMax - size;
            }

            return lotMin + _random.Next(0, lotMax - lotMin - size + 1);
        }

        private void AddWithCentrePiece(GridRect area, PlotUse use, List<Plot> plots, int pieceCells = WellCells)
        {
            if (area.Width < MinCourtyardForFountain || area.Height < MinCourtyardForFountain)
            {
                plots.Add(new Plot(area, 1, use));
                return;
            }

            GridRect piece = RectCarving.Centred(area, pieceCells, pieceCells);
            plots.Add(new Plot(piece, 1, PlotUse.Fountain));
            AddAround(area, piece, use, plots);
        }

        private static void AddAround(GridRect outer, GridRect hole, PlotUse use, List<Plot> plots)
        {
            foreach (GridRect part in RectCarving.Around(outer, hole))
            {
                plots.Add(new Plot(part, 1, use));
            }
        }
    }
}
