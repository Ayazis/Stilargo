using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Scavenging;
using Stilargo.Tests.EditMode.Garrison;

namespace Stilargo.Tests.EditMode.Fortification
{
    /// <summary>
    /// The garrison test city's street plot as a building: with default layout settings it has
    /// four roof slots and two windows per storey on its single road facade.
    /// </summary>
    internal static class FortificationTestHouse
    {
        // One shared instance, because FortificationSettings.Default builds new definitions on every call.
        public static readonly FortificationSettings Settings = FortificationSettings.Default;

        public static GarrisonBuilding CreateBuilding()
        {
            IReadOnlyList<GarrisonBuilding> buildings = new GarrisonSlotGenerator(new GarrisonLayoutSettings()).Generate(GarrisonTestCity.Create());
            return buildings.Single(building => building.Plot == GarrisonTestCity.StreetPlot);
        }

        public static HouseClaim CreateClaim(DropOff dropOff)
        {
            return new HouseClaim(Settings, new FortificationSlotLayout(GarrisonTestCity.CellSize), dropOff);
        }

        public static DropOff CreateDropOff()
        {
            return new DropOff(new WorldPoint(0f, 0f), new Wallet());
        }

        public static FortificationDefinition Find(string id)
        {
            return Settings.Definitions.Single(definition => definition.Id == id);
        }
    }
}
