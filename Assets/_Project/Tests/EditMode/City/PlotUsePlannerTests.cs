using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class PlotUsePlannerTests
    {
        private const int Seed = 1234;

        private static CityLayout Generate()
        {
            return new CityGenerator(Seed, new CityConfig()).Generate();
        }

        [Test]
        public void Plan_PlacesEveryLandmarkTheRulesAskFor()
        {
            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(Generate(), Seed);

            Assert.AreEqual(DistrictSettings.DefaultChurchCount, city.Plots.Count(plot => plot.Use == PlotUse.Church));
            Assert.AreEqual(DistrictSettings.DefaultSchoolCount, city.Plots.Count(plot => plot.Use == PlotUse.School));
            Assert.AreEqual(DistrictSettings.DefaultTavernCount, city.Plots.Count(plot => plot.Use == PlotUse.Tavern));
            Assert.AreEqual(DistrictSettings.DefaultWaterCount, city.Plots.Count(plot => plot.Use == PlotUse.Water));
        }

        [Test]
        public void Plan_PutsTheChurchAcrossTheStreetFromTheMarketSquare()
        {
            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(Generate(), Seed);
            GridRect square = city.Plots.Single(plot => plot.Use == PlotUse.Market).Area;
            Plot church = city.Plots.Single(plot => plot.Use == PlotUse.Church);
            int roadWidth = new CityConfig().RoadWidth;

            Assert.AreEqual(roadWidth, Gap(square, church.Area), $"Church {church.Area} does not face the square {square}.");
            Assert.IsTrue(church.HasTower);
            Assert.AreEqual(DistrictSettings.DefaultChurchTowerStoreys, church.TowerStoreys);
        }

        [Test]
        public void Plan_KeepsStreetsAndPlotAreas()
        {
            CityLayout generated = Generate();

            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(generated, Seed);

            Assert.AreEqual(generated.Plots.Count, city.Plots.Count);
            CollectionAssert.AreEqual(generated.Plots.Select(plot => plot.Area), city.Plots.Select(plot => plot.Area));
            CollectionAssert.AreEqual(generated.RoadCells(), city.RoadCells());
        }

        [Test]
        public void Plan_IsTheSameForTheSameSeed()
        {
            var planner = new PlotUsePlanner(new DistrictSettings());

            CityLayout first = planner.Plan(Generate(), Seed);
            CityLayout second = planner.Plan(Generate(), Seed);

            CollectionAssert.AreEqual(first.Plots.Select(plot => plot.Use), second.Plots.Select(plot => plot.Use));
        }

        [Test]
        public void Plan_WithMoreLandmarksThanPlots_StopsWhenPlotsRunOut()
        {
            CityLayout generated = Generate();
            var rules = new[] { new PlotUseRule(PlotUse.Park, generated.Plots.Count + 5, PlotPlacement.Anywhere) };

            CityLayout city = new PlotUsePlanner(new DistrictSettings(rules)).Plan(generated, Seed);

            Assert.IsFalse(city.Plots.Any(plot => plot.Use == PlotUse.House));
            Assert.AreEqual(1, city.Plots.Count(plot => plot.Use == PlotUse.Market));
        }

        [Test]
        public void Plan_CountsAChurchTheTownPlanAlreadyHas()
        {
            CityLayout town = new Core.City.CanalTown.CanalTownGenerator(Seed, new Core.City.CanalTown.CanalTownSettings()).Generate().Layout;
            int churchesBefore = town.Plots.Count(plot => plot.Use == PlotUse.Church);

            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(town, Seed);

            Assert.AreEqual(1, churchesBefore);
            Assert.AreEqual(DistrictSettings.DefaultChurchCount, city.Plots.Count(plot => plot.Use == PlotUse.Church));
            Assert.IsTrue(city.Plots.Single(plot => plot.Use == PlotUse.Church).HasTower);
        }

        [Test]
        public void Plan_LeavesEveryOpenPlotAlone()
        {
            CityLayout generated = Generate();

            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(generated, Seed);

            Assert.AreEqual(generated.Plots.Count, city.Plots.Count);
            for (int index = 0; index < generated.Plots.Count; index++)
            {
                Plot before = generated.Plots[index];
                if (before.Use == PlotUse.House)
                {
                    continue;
                }

                Assert.AreEqual(before.Area, city.Plots[index].Area);
                Assert.AreEqual(before.Use, city.Plots[index].Use);
            }
        }

        [Test]
        public void DistrictSettings_RejectsARuleForTheMarket()
        {
            var rules = new[] { new PlotUseRule(PlotUse.Market, 1, PlotPlacement.Anywhere) };

            Assert.That(() => new DistrictSettings(rules), Throws.ArgumentException);
        }

        private static int Gap(GridRect first, GridRect second)
        {
            int gapX = System.Math.Max(0, System.Math.Max(first.Min.X - second.XMax, second.Min.X - first.XMax));
            int gapY = System.Math.Max(0, System.Math.Max(first.Min.Y - second.YMax, second.Min.Y - first.YMax));
            return gapX + gapY;
        }

        [Test]
        public void ParksAndWater_HaveNoBuilding()
        {
            CityLayout city = new PlotUsePlanner(new DistrictSettings()).Plan(Generate(), Seed);

            Assert.IsTrue(city.Plots.Where(plot => plot.Use == PlotUse.Park || plot.Use == PlotUse.Water).All(plot => !plot.HasBuilding));
            Assert.IsTrue(city.Plots.Where(plot => plot.Use == PlotUse.Church).All(plot => plot.HasBuilding));
        }

        [Test]
        public void DistrictSettings_RejectsARuleForHouses()
        {
            var rules = new[] { new PlotUseRule(PlotUse.House, 1, PlotPlacement.Anywhere) };

            Assert.That(() => new DistrictSettings(rules), Throws.ArgumentException);
        }

        [Test]
        public void PlotUseRule_RejectsATowerNoTallerThanTheBuilding()
        {
            Assert.That(() => new PlotUseRule(PlotUse.Church, 1, PlotPlacement.Largest).WithHeight(3, 3), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }
    }
}
