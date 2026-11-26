using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    /// <summary>Road queries shared by the city tests.</summary>
    internal static class CityRoads
    {
        private static readonly GridPosition[] Steps =
        {
            new GridPosition(1, 0),
            new GridPosition(-1, 0),
            new GridPosition(0, 1),
            new GridPosition(0, -1),
        };

        public static bool AreConnected(CityLayout layout)
        {
            List<GridPosition> roads = layout.RoadCells().ToList();
            var reached = new HashSet<GridPosition> { roads[0] };
            var open = new Queue<GridPosition>(reached);
            while (open.Count > 0)
            {
                GridPosition cell = open.Dequeue();
                foreach (GridPosition step in Steps)
                {
                    var next = new GridPosition(cell.X + step.X, cell.Y + step.Y);
                    if (layout.IsRoad(next) && reached.Add(next))
                    {
                        open.Enqueue(next);
                    }
                }
            }

            return reached.Count == roads.Count;
        }

        public static GridPosition CellAtMetres(CityLayout layout, float x, float z)
        {
            return new GridPosition((int)(x / layout.CellSize), (int)(z / layout.CellSize));
        }
    }
}
