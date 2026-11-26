using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>houseBlock</c> section of <c>Town.json</c>. Defaults match a new <see cref="RowHouseSettings"/>.</summary>
    [Serializable]
    public sealed class RowHouseEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int rowDepthCells = RowHouseSettings.DefaultRowDepthCells;
        [SerializeField] private int minFrontageCells = RowHouseSettings.DefaultMinFrontageCells;
        [SerializeField] private int maxFrontageCells = RowHouseSettings.DefaultMaxFrontageCells;
        [SerializeField] private int minStoreys = RowHouseSettings.DefaultMinStoreys;
        [SerializeField] private int maxStoreys = RowHouseSettings.DefaultMaxStoreys;

        public RowHouseSettings ToSettings()
        {
            return new RowHouseSettings(
                rowDepthCells,
                new IntRange(minFrontageCells, maxFrontageCells),
                new IntRange(minStoreys, maxStoreys));
        }
    }
}
