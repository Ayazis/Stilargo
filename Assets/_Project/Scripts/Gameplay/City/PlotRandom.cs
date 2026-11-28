using Stilargo.Core.City;

namespace Stilargo.Gameplay.City
{
    /// <summary>A random source seeded by a plot's position, so the same plot is always dressed the same way.</summary>
    internal static class PlotRandom
    {
        private const int SeedXPrime = 73856093;
        private const int SeedYPrime = 19349663;

        public static System.Random For(Plot plot)
        {
            return new System.Random((plot.Area.Min.X * SeedXPrime) ^ (plot.Area.Min.Y * SeedYPrime));
        }
    }
}
