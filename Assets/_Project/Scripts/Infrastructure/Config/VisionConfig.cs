using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Vision.json</c>. Defaults match a new <see cref="VisionSettings"/>.</summary>
    [Serializable]
    public sealed class VisionConfig
    {
        public const string FileName = "Vision.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private float unitVisionRadiusMetres = VisionSettings.DefaultUnitVisionRadiusMetres;
        [SerializeField] private float eyeHeightMetres = VisionSettings.DefaultEyeHeightMetres;
        [SerializeField] private float sightMetresPerMetreOfHeight = VisionSettings.DefaultSightMetresPerMetreOfHeight;
        [SerializeField] private float maxVisionRadiusMetres = VisionSettings.DefaultMaxVisionRadiusMetres;
        [SerializeField] private int fogCellsPerCityCell = VisionSettings.DefaultFogCellsPerCityCell;

        public static VisionSettings Load()
        {
            return ConfigFile.Read<VisionConfig>(FileName).ToSettings();
        }

        public VisionSettings ToSettings()
        {
            try
            {
                return new VisionSettings()
                    .WithUnitVision(unitVisionRadiusMetres, eyeHeightMetres)
                    .WithHeightBonus(sightMetresPerMetreOfHeight, maxVisionRadiusMetres)
                    .WithFogResolution(fogCellsPerCityCell);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
