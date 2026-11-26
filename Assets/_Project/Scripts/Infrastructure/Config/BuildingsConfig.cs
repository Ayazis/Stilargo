using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Buildings.json</c>: how many of each landmark the city gets. Defaults match <see cref="DistrictSettings"/>.</summary>
    [Serializable]
    public sealed class BuildingsConfig
    {
        public const string FileName = "Buildings.json";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private PlotUseEntry church = PlotUseEntry.DefaultsFor(PlotUse.Church);
        [SerializeField] private PlotUseEntry school = PlotUseEntry.DefaultsFor(PlotUse.School);
        [SerializeField] private PlotUseEntry tavern = PlotUseEntry.DefaultsFor(PlotUse.Tavern);
        [SerializeField] private PlotUseEntry water = PlotUseEntry.DefaultsFor(PlotUse.Water);

        public static DistrictSettings Load()
        {
            return ConfigFile.Read<BuildingsConfig>(FileName).ToSettings();
        }

        // Rule order is pick order: the church takes the largest plot on the market square, the school the largest
        // elsewhere, the taverns the next ones on the square; the rest are scattered. Parks are whole blocks (Town.json).
        public DistrictSettings ToSettings()
        {
            try
            {
                return new DistrictSettings(new[]
                {
                    church.ToRule(PlotUse.Church, PlotPlacement.FacingSquare),
                    school.ToRule(PlotUse.School, PlotPlacement.Largest),
                    tavern.ToRule(PlotUse.Tavern, PlotPlacement.FacingSquare),
                    water.ToRule(PlotUse.Water, PlotPlacement.Anywhere),
                });
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
