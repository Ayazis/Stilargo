using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>One landmark section of <c>Buildings.json</c>, e.g. <c>church</c>.</summary>
    [Serializable]
    public sealed class PlotUseEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int count;
        [SerializeField] private int storeys;
        [SerializeField] private int towerStoreys;
        [SerializeField] private float incomeMultiplier = Plot.StandardIncomeMultiplier;

        public static PlotUseEntry DefaultsFor(PlotUse use)
        {
            foreach (PlotUseRule rule in DistrictSettings.DefaultRules())
            {
                if (rule.Use == use)
                {
                    return new PlotUseEntry
                    {
                        count = rule.Count,
                        storeys = rule.Storeys,
                        towerStoreys = rule.TowerStoreys,
                        incomeMultiplier = rule.IncomeMultiplier,
                    };
                }
            }

            throw new ArgumentOutOfRangeException(nameof(use), use, "No default rule for this use.");
        }

        public PlotUseRule ToRule(PlotUse use, PlotPlacement placement)
        {
            return new PlotUseRule(use, count, placement)
                .WithHeight(storeys, towerStoreys)
                .WithIncomeMultiplier(incomeMultiplier);
        }
    }
}
