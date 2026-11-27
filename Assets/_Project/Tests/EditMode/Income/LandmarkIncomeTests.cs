using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Income;
using Stilargo.Tests.EditMode.Fortification;
using Stilargo.Tests.EditMode.Garrison;

namespace Stilargo.Tests.EditMode.Income
{
    public sealed class LandmarkIncomeTests
    {
        private const float TavernMultiplier = 1.8f;

        private static ClaimedHouse ClaimHouseOn(Plot plot)
        {
            CityLayout city = GarrisonTestCity.Create().WithPlots(new List<Plot> { plot });
            GarrisonBuilding building = new GarrisonSlotGenerator(new GarrisonLayoutSettings()).Generate(city).Single();
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            Assert.IsTrue(claim.TryClaim(building));
            return claim.House;
        }

        [Test]
        public void Tavern_EarnsItsMultipleOfAnEqualHouse()
        {
            Plot house = GarrisonTestCity.StreetPlot;
            Plot tavern = house.UsedFor(new PlotUseRule(PlotUse.Tavern, 1, PlotPlacement.Anywhere).WithIncomeMultiplier(TavernMultiplier));
            var rate = new HouseIncomeRate(HouseIncomeSettings.Default);

            float houseIncome = rate.MoneyPerSecond(ClaimHouseOn(house));
            float tavernIncome = rate.MoneyPerSecond(ClaimHouseOn(tavern));

            Assert.AreEqual(houseIncome * TavernMultiplier, tavernIncome, 0.0001f);
        }
    }
}
