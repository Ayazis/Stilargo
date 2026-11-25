using System;
using System.Linq;
using Stilargo.Core.Fortification;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Fortification.json</c>. Defaults match <see cref="FortificationSettings.Default"/>.</summary>
    [Serializable]
    public sealed class FortificationConfig
    {
        public const string FileName = "Fortification.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float houseBaseHealthPoints = FortificationSettings.DefaultHouseBaseHealthPoints;
        [SerializeField] private FortificationEntry[] fortifications = FortificationEntry.Defaults();

        public static FortificationSettings Load()
        {
            return ConfigFile.Read<FortificationConfig>(FileName).ToSettings();
        }

        public FortificationSettings ToSettings()
        {
            try
            {
                return new FortificationSettings(houseBaseHealthPoints, fortifications.Select(entry => entry.ToDefinition()).ToList());
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
