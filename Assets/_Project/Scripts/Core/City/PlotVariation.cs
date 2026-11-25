using System;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Picks a stable look variant for a plot, so the same seed always paints the same city
    /// and neighbouring buildings rarely share a colour.
    /// </summary>
    public static class PlotVariation
    {
        private const int XPrime = 73856093;
        private const int YPrime = 19349663;
        private const int StoreysPrime = 83492791;

        public static int Pick(Plot plot, int variantCount)
        {
            if (plot == null)
            {
                throw new ArgumentNullException(nameof(plot));
            }

            if (variantCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(variantCount), variantCount, "Need at least one variant.");
            }

            unchecked
            {
                int hash = (plot.Area.Min.X * XPrime) ^ (plot.Area.Min.Y * YPrime) ^ (plot.Storeys * StoreysPrime);
                return (int)((uint)hash % (uint)variantCount);
            }
        }
    }
}
