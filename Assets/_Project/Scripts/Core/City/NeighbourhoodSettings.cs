using System;

namespace Stilargo.Core.City
{
    /// <summary>
    /// What the outskirts blocks become: a few small plazas, then a weighted mix of dense old town, villas with
    /// gardens and low workshops with yards. From the <c>neighbourhoods</c> section of <c>Town.json</c>.
    /// </summary>
    public sealed class NeighbourhoodSettings
    {
        public const int DefaultPlazaCount = 1;
        public const int DefaultOldTownWeight = 3;
        public const int DefaultVillasWeight = 2;
        public const int DefaultWorkshopsWeight = 1;
        public const float DefaultVillaGardenShare = 0.4f;
        public const float DefaultWorkshopYardShare = 0.35f;
        public const int DefaultVillaStoreys = 2;
        public const int DefaultWorkshopStoreys = 1;

        public int PlazaCount { get; set; } = DefaultPlazaCount;

        public int OldTownWeight { get; set; } = DefaultOldTownWeight;

        public int VillasWeight { get; set; } = DefaultVillasWeight;

        public int WorkshopsWeight { get; set; } = DefaultWorkshopsWeight;

        /// <summary>Share (0–1) of villa plots left as a garden between the houses.</summary>
        public float VillaGardenShare { get; set; } = DefaultVillaGardenShare;

        /// <summary>Share (0–1) of workshop plots left as an open yard.</summary>
        public float WorkshopYardShare { get; set; } = DefaultWorkshopYardShare;

        public int VillaStoreys { get; set; } = DefaultVillaStoreys;

        public int WorkshopStoreys { get; set; } = DefaultWorkshopStoreys;

        public int TotalWeight => OldTownWeight + VillasWeight + WorkshopsWeight;

        public NeighbourhoodSettings Validate()
        {
            RequireNotNegative(PlazaCount, nameof(PlazaCount));
            RequireNotNegative(OldTownWeight, nameof(OldTownWeight));
            RequireNotNegative(VillasWeight, nameof(VillasWeight));
            RequireNotNegative(WorkshopsWeight, nameof(WorkshopsWeight));
            if (TotalWeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(OldTownWeight), TotalWeight, "At least one neighbourhood weight must be positive.");
            }

            Share.Require(VillaGardenShare, nameof(VillaGardenShare));
            Share.Require(WorkshopYardShare, nameof(WorkshopYardShare));
            RequirePositive(VillaStoreys, nameof(VillaStoreys));
            RequirePositive(WorkshopStoreys, nameof(WorkshopStoreys));
            return this;
        }

        private static void RequireNotNegative(int value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Must not be negative.");
            }
        }

        private static void RequirePositive(int value, string name)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be positive.");
            }
        }
    }
}
