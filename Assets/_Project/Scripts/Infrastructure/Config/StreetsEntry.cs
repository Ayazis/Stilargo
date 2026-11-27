using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>streets</c> section of <c>Town.json</c>. Defaults match a new <see cref="StreetSettings"/>.</summary>
    [Serializable]
    public sealed class StreetsEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int mergedBlockPairs = StreetSettings.DefaultMergedBlockPairs;
        [SerializeField] private float alleyChance = StreetSettings.DefaultAlleyChance;
        [SerializeField] private int alleyWidthCells = StreetSettings.DefaultAlleyWidthCells;

        public StreetSettings ToSettings()
        {
            return new StreetSettings
            {
                MergedBlockPairs = mergedBlockPairs,
                AlleyChance = alleyChance,
                AlleyWidthCells = alleyWidthCells,
            };
        }
    }
}
