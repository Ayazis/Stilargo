using System;
using System.Collections.Generic;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Decides, from what lies around each building, which way it faces and what kind of house it is. A building
    /// faces a square if it has one alongside, otherwise the side with the most street. A house beside a yard is a
    /// workshop, or a farmhouse when no other house touches it; a free-standing house is detached; one facing a
    /// square is a townhouse; one with street on two sides is a corner house; the rest are row houses.
    /// </summary>
    public static class HouseStyler
    {
        private const int CornerStreetSides = 2;

        // Ties between sides go to the earlier one, so a building with no clear front faces south.
        private static readonly Facing[] Sides = { Facing.South, Facing.North, Facing.East, Facing.West };

        public static CityLayout Style(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            Dictionary<GridPosition, Plot> plotAt = MapCells(layout.Plots);
            List<Plot> styled = layout.Plots.Select(plot => plot.HasBuilding ? StyleOne(plot, layout, plotAt) : plot).ToList();
            return layout.WithPlots(styled);
        }

        private static Dictionary<GridPosition, Plot> MapCells(IEnumerable<Plot> plots)
        {
            var plotAt = new Dictionary<GridPosition, Plot>();
            foreach (Plot plot in plots)
            {
                foreach (GridPosition cell in plot.Area.Cells())
                {
                    plotAt[cell] = plot;
                }
            }

            return plotAt;
        }

        private static Plot StyleOne(Plot plot, CityLayout layout, Dictionary<GridPosition, Plot> plotAt)
        {
            Dictionary<Facing, Edge> edges = Sides.ToDictionary(side => side, side => new Edge(Outside(plot.Area, side), layout, plotAt));
            Facing front = FrontOf(edges);
            return plot.Styled(StyleOf(edges, front), front);
        }

        private static Facing FrontOf(Dictionary<Facing, Edge> edges)
        {
            Facing square = Sides.OrderByDescending(side => edges[side].SquareCells).First();
            if (edges[square].SquareCells > 0)
            {
                return square;
            }

            return Sides.OrderByDescending(side => edges[side].RoadCells).First();
        }

        private static HouseStyle StyleOf(Dictionary<Facing, Edge> edges, Facing front)
        {
            bool touchesHouse = edges.Values.Any(edge => edge.HouseCells > 0);
            if (edges.Values.Any(edge => edge.YardCells > 0))
            {
                return touchesHouse ? HouseStyle.Workshop : HouseStyle.Farmhouse;
            }

            if (!touchesHouse)
            {
                return HouseStyle.Detached;
            }

            if (edges[front].SquareCells > 0)
            {
                return HouseStyle.Townhouse;
            }

            return edges.Values.Count(edge => edge.RoadCells > 0) >= CornerStreetSides ? HouseStyle.Corner : HouseStyle.Row;
        }

        // The row of cells just outside one side of a rectangle.
        private static IEnumerable<GridPosition> Outside(GridRect area, Facing side)
        {
            switch (side)
            {
                case Facing.South:
                    return Enumerable.Range(area.Min.X, area.Width).Select(x => new GridPosition(x, area.Min.Y - 1));
                case Facing.North:
                    return Enumerable.Range(area.Min.X, area.Width).Select(x => new GridPosition(x, area.YMax));
                case Facing.West:
                    return Enumerable.Range(area.Min.Y, area.Height).Select(y => new GridPosition(area.Min.X - 1, y));
                default:
                    return Enumerable.Range(area.Min.Y, area.Height).Select(y => new GridPosition(area.XMax, y));
            }
        }

        /// <summary>What lies along one side of a building, counted in cells.</summary>
        private sealed class Edge
        {
            public Edge(IEnumerable<GridPosition> cells, CityLayout layout, Dictionary<GridPosition, Plot> plotAt)
            {
                foreach (GridPosition cell in cells.Where(layout.IsInside))
                {
                    Count(cell, layout, plotAt);
                }
            }

            public int RoadCells { get; private set; }

            public int SquareCells { get; private set; }

            public int HouseCells { get; private set; }

            public int YardCells { get; private set; }

            private void Count(GridPosition cell, CityLayout layout, Dictionary<GridPosition, Plot> plotAt)
            {
                if (layout.IsRoad(cell))
                {
                    RoadCells++;
                    return;
                }

                if (!plotAt.TryGetValue(cell, out Plot neighbour))
                {
                    return;
                }

                if (neighbour.HasBuilding)
                {
                    HouseCells++;
                }
                else if (neighbour.Use == PlotUse.Yard)
                {
                    YardCells++;
                }
                else if (neighbour.Use == PlotUse.Plaza || neighbour.Use == PlotUse.Market)
                {
                    SquareCells++;
                }
            }
        }
    }
}
