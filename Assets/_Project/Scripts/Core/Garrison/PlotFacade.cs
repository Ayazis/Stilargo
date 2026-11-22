using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.City;

namespace Stilargo.Core.Garrison
{
    /// <summary>
    /// One side of a plot's footprint at ground level, traversed counter-clockwise seen from above
    /// so the outward normal is always Up x Along.
    /// </summary>
    internal sealed class PlotFacade
    {
        private PlotFacade(Vector3 start, Vector3 end, IReadOnlyList<GridPosition> adjacentCells)
        {
            Start = start;
            End = end;
            AdjacentCells = adjacentCells;
            Outward = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, end - start));
        }

        public Vector3 Start { get; }

        public Vector3 End { get; }

        public Vector3 Outward { get; }

        /// <summary>The cells just outside this side, in traversal order.</summary>
        public IReadOnlyList<GridPosition> AdjacentCells { get; }

        /// <summary>South, east, north, west.</summary>
        public static IReadOnlyList<PlotFacade> AllOf(GridRect area, float cellSize)
        {
            float west = area.Min.X * cellSize;
            float east = area.XMax * cellSize;
            float south = area.Min.Y * cellSize;
            float north = area.YMax * cellSize;
            IEnumerable<int> columns = Enumerable.Range(area.Min.X, area.Width);
            IEnumerable<int> rows = Enumerable.Range(area.Min.Y, area.Height);

            return new[]
            {
                new PlotFacade(new Vector3(west, 0f, south), new Vector3(east, 0f, south), columns.Select(x => new GridPosition(x, area.Min.Y - 1)).ToList()),
                new PlotFacade(new Vector3(east, 0f, south), new Vector3(east, 0f, north), rows.Select(y => new GridPosition(area.XMax, y)).ToList()),
                new PlotFacade(new Vector3(east, 0f, north), new Vector3(west, 0f, north), columns.Reverse().Select(x => new GridPosition(x, area.YMax)).ToList()),
                new PlotFacade(new Vector3(west, 0f, north), new Vector3(west, 0f, south), rows.Reverse().Select(y => new GridPosition(area.Min.X - 1, y)).ToList()),
            };
        }

        public bool FacesRoad(CityLayout layout)
        {
            return AdjacentCells.Any(layout.IsRoad);
        }
    }
}
