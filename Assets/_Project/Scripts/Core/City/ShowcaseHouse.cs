using System;

namespace Stilargo.Core.City
{
    /// <summary>
    /// The one house that tries out the concept-art look. It stands on its own just south of the city, the side
    /// the starting camera looks from, so it is easy to find and compare with the city's houses.
    /// </summary>
    public static class ShowcaseHouse
    {
        private const int WidthCells = 6;
        private const int DepthCells = 5;
        private const int Storeys = 3;
        private const int GapCells = 4;

        public static Plot BesideCity(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            int west = (layout.Width - WidthCells) / 2;
            int south = -(GapCells + DepthCells);
            return new Plot(new GridRect(new GridPosition(west, south), WidthCells, DepthCells), Storeys);
        }
    }
}
