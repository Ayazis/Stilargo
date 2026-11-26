using System;

namespace Stilargo.Core.City
{
    /// <summary>How many plots get one use, where they go and how they are built. Loaded from <c>Buildings.json</c>.</summary>
    public sealed class PlotUseRule
    {
        public PlotUseRule(PlotUse use, int count, PlotPlacement placement)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, $"{use}: count must not be negative.");
            }

            Use = use;
            Count = count;
            Placement = placement;
        }

        public PlotUse Use { get; }

        public int Count { get; }

        public PlotPlacement Placement { get; }

        /// <summary>Storeys the building gets; 0 keeps the plot's generated height.</summary>
        public int Storeys { get; private set; }

        /// <summary>Storeys of a bell tower; 0 for none. Must rise above the building.</summary>
        public int TowerStoreys { get; private set; }

        public float IncomeMultiplier { get; private set; } = Plot.StandardIncomeMultiplier;

        public PlotUseRule WithHeight(int storeys, int towerStoreys)
        {
            if (storeys < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storeys), storeys, $"{Use}: storeys must not be negative.");
            }

            if (towerStoreys < 0 || (towerStoreys > 0 && towerStoreys <= storeys))
            {
                throw new ArgumentOutOfRangeException(nameof(towerStoreys), towerStoreys, $"{Use}: a tower must be taller than the building.");
            }

            Storeys = storeys;
            TowerStoreys = towerStoreys;
            return this;
        }

        public PlotUseRule WithIncomeMultiplier(float multiplier)
        {
            if (!(multiplier >= 0f) || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, $"{Use}: income multiplier must be a non-negative number.");
            }

            IncomeMultiplier = multiplier;
            return this;
        }
    }
}
