using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>streetLife</c> section of <c>Town.json</c>. Defaults match a new <see cref="StreetLifeSettings"/>.</summary>
    [Serializable]
    public sealed class StreetLifeEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int lampSpacingCells = StreetLifeSettings.DefaultLampSpacingCells;
        [SerializeField] private int treeSpacingCells = StreetLifeSettings.DefaultTreeSpacingCells;
        [SerializeField] private float clutterChance = StreetLifeSettings.DefaultClutterChance;

        public StreetLifeSettings ToSettings()
        {
            return new StreetLifeSettings
            {
                LampSpacingCells = lampSpacingCells,
                TreeSpacingCells = treeSpacingCells,
                ClutterChance = clutterChance,
            };
        }
    }
}
