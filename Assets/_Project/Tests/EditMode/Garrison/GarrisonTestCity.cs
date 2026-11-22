using System.Collections.Generic;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.Garrison
{
    /// <summary>
    /// 6x6 grid of 2 m cells with a road along row 0. The street plot (cells 1-2, 1-2, two storeys) touches
    /// the road on its south side only; the back plot (cells 1-2, 4-5, one storey) touches no road.
    /// </summary>
    internal static class GarrisonTestCity
    {
        public const float CellSize = 2f;
        public const int GridSize = 6;
        public const int StreetPlotStoreys = 2;

        public static readonly Plot StreetPlot = new Plot(new GridRect(new GridPosition(1, 1), 2, 2), StreetPlotStoreys);
        public static readonly Plot BackPlot = new Plot(new GridRect(new GridPosition(1, 4), 2, 2), 1);

        public static CityLayout Create()
        {
            CityGrid grid = new CityGrid(GridSize, GridSize, CellSize);
            for (int x = 0; x < GridSize; x++)
            {
                grid.MarkRoad(new GridPosition(x, 0));
            }

            return new CityLayout(grid, new List<GridRect>(), new List<Plot> { StreetPlot, BackPlot });
        }
    }
}
