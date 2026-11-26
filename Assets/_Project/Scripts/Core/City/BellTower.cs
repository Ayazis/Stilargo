using System;

namespace Stilargo.Core.City
{
    /// <summary>Where a plot's bell tower stands: a square at the plot's south-west corner, shared by the builder, garrison and sight.</summary>
    public static class BellTower
    {
        public const int EdgeCells = 2;

        public static GridRect AreaOf(Plot plot)
        {
            if (plot == null)
            {
                throw new ArgumentNullException(nameof(plot));
            }

            if (!plot.HasTower)
            {
                throw new InvalidOperationException($"The {plot.Use} at {plot.Area} has no tower.");
            }

            int width = Math.Min(EdgeCells, plot.Area.Width);
            int height = Math.Min(EdgeCells, plot.Area.Height);
            return new GridRect(plot.Area.Min, width, height);
        }
    }
}
