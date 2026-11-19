using System;

namespace Stilargo.Core.City
{
    /// <summary>A building plot: the cells one building occupies and how tall it is.</summary>
    public sealed class Plot
    {
        public Plot(GridRect area, int storeys)
        {
            if (storeys <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storeys), storeys, "Storeys must be positive.");
            }

            Area = area;
            Storeys = storeys;
        }

        public GridRect Area { get; }

        public int Storeys { get; }
    }
}
