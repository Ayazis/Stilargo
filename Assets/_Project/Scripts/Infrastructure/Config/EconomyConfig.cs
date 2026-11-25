using System;
using Stilargo.Core.Economy;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Economy.json</c>. Defaults match a new <see cref="EconomySettings"/>.</summary>
    [Serializable]
    public sealed class EconomyConfig
    {
        public const string FileName = "Economy.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int startingMoney = EconomySettings.DefaultStartingMoney;

        public static EconomySettings Load()
        {
            return ConfigFile.Read<EconomyConfig>(FileName).ToSettings();
        }

        public EconomySettings ToSettings()
        {
            try
            {
                return new EconomySettings(startingMoney);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
