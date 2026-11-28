using System;
using System.Linq;

namespace Stilargo.Core.City
{
    /// <summary>
    /// The one house that tries out the concept-art look. It takes over the southernmost field or meadow nearest the
    /// middle, the side the starting camera looks from, so it is inside the map (the fog of war can reveal it) and easy
    /// to find and compare with the city's houses.
    /// </summary>
    public static class ShowcaseHouse
    {
        private const int WidthCells = 6;
        private const int DepthCells = 5;
        private const int Storeys = 3;

        public static ShowcaseSite Place(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            float middle = layout.Width / 2f;
            Plot site = layout.Plots
                .Where(IsOpenCountryside)
                .Where(plot => plot.Area.Width >= WidthCells && plot.Area.Height >= DepthCells)
                .OrderBy(plot => plot.Area.Min.Y)
                .ThenBy(plot => Math.Abs(plot.Area.Min.X + (plot.Area.Width / 2f) - middle))
                .FirstOrDefault();
            if (site == null)
            {
                throw new InvalidOperationException($"No field or meadow of at least {WidthCells}x{DepthCells} cells for the showcase house.");
            }

            return new ShowcaseSite(site, new Plot(CentredIn(site.Area), Storeys));
        }

        private static bool IsOpenCountryside(Plot plot)
        {
            return plot.Use == PlotUse.Field || plot.Use == PlotUse.Meadow;
        }

        private static GridRect CentredIn(GridRect area)
        {
            int x = area.Min.X + ((area.Width - WidthCells) / 2);
            int y = area.Min.Y + ((area.Height - DepthCells) / 2);
            return new GridRect(new GridPosition(x, y), WidthCells, DepthCells);
        }
    }
}
