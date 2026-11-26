using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>neighbourhoods</c> section of <c>Town.json</c>. Defaults match a new <see cref="NeighbourhoodSettings"/>.</summary>
    [Serializable]
    public sealed class NeighbourhoodsEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int plazaCount = NeighbourhoodSettings.DefaultPlazaCount;
        [SerializeField] private int oldTownWeight = NeighbourhoodSettings.DefaultOldTownWeight;
        [SerializeField] private int villasWeight = NeighbourhoodSettings.DefaultVillasWeight;
        [SerializeField] private int workshopsWeight = NeighbourhoodSettings.DefaultWorkshopsWeight;
        [SerializeField] private float villaGardenShare = NeighbourhoodSettings.DefaultVillaGardenShare;
        [SerializeField] private float workshopYardShare = NeighbourhoodSettings.DefaultWorkshopYardShare;
        [SerializeField] private int villaStoreys = NeighbourhoodSettings.DefaultVillaStoreys;
        [SerializeField] private int workshopStoreys = NeighbourhoodSettings.DefaultWorkshopStoreys;

        public NeighbourhoodSettings ToSettings()
        {
            return new NeighbourhoodSettings
            {
                PlazaCount = plazaCount,
                OldTownWeight = oldTownWeight,
                VillasWeight = villasWeight,
                WorkshopsWeight = workshopsWeight,
                VillaGardenShare = villaGardenShare,
                WorkshopYardShare = workshopYardShare,
                VillaStoreys = villaStoreys,
                WorkshopStoreys = workshopStoreys,
            };
        }
    }
}
