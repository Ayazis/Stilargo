using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Town.json</c>: the town plan around the market square. Defaults match a new <see cref="TownSettings"/>.</summary>
    [Serializable]
    public sealed class TownConfig
    {
        public const string FileName = "Town.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int parkBlocks = TownSettings.DefaultParkBlockCount;
        [SerializeField] private RowHouseEntry houseBlock = new RowHouseEntry();

        public static TownSettings Load()
        {
            return ConfigFile.Read<TownConfig>(FileName).ToSettings();
        }

        public TownSettings ToSettings()
        {
            try
            {
                return new TownSettings(parkBlocks, houseBlock.ToSettings());
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
