using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;
using Stilargo.Core.Hill;

namespace Stilargo.Tests.EditMode.City.CanalTown
{
    public sealed class CanalTownGeneratorTests
    {
        private static readonly int[] Seeds = { 0, 1, 1234, 98765 };

        private static Stilargo.Core.City.CanalTown.CanalTown Generate(int seed)
        {
            return new CanalTownGenerator(seed, new CanalTownSettings()).Generate();
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_IsTheSameForTheSameSeed(int seed)
        {
            List<Plot> first = Generate(seed).Layout.Plots.ToList();
            List<Plot> second = Generate(seed).Layout.Plots.ToList();

            Assert.AreEqual(first.Count, second.Count);
            for (int index = 0; index < first.Count; index++)
            {
                Assert.AreEqual(first[index].Area, second[index].Area);
                Assert.AreEqual(first[index].Use, second[index].Use);
                Assert.AreEqual(first[index].Storeys, second[index].Storeys);
            }
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_GivesEveryCellExactlyOneUse(int seed)
        {
            CityLayout layout = Generate(seed).Layout;
            var owners = new int[layout.Width, layout.Height];
            foreach (GridPosition cell in layout.Plots.SelectMany(plot => plot.Area.Cells()))
            {
                Assert.IsTrue(layout.IsInside(cell), $"{cell} lies outside the map.");
                Assert.IsFalse(layout.IsRoad(cell), $"{cell} is both a road and a plot.");
                owners[cell.X, cell.Y]++;
            }

            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    int uses = owners[x, y] + (layout.IsRoad(new GridPosition(x, y)) ? 1 : 0);
                    Assert.AreEqual(1, uses, $"Cell ({x}, {y}) has {uses} uses.");
                }
            }
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_ConnectsEveryRoad(int seed)
        {
            Assert.IsTrue(CityRoads.AreConnected(Generate(seed).Layout));
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_LetsThePlayerEnterOnTheWestEdgeAndTheRivalOnTheEast(int seed)
        {
            Stilargo.Core.City.CanalTown.CanalTown town = Generate(seed);

            Assert.AreEqual(0, town.PlayerEntry.X);
            Assert.AreEqual(town.Layout.Width - 1, town.RivalEntry.X);
            Assert.IsTrue(town.Layout.IsRoad(town.PlayerEntry));
            Assert.IsTrue(town.Layout.IsRoad(town.RivalEntry));
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_LetsEveryBuildingBeEnteredFromARoad(int seed)
        {
            CityLayout layout = Generate(seed).Layout;
            foreach (Plot building in layout.Plots.Where(plot => plot.HasBuilding))
            {
                Assert.IsTrue(Neighbours(building.Area).Any(layout.IsRoad), $"Building at {building.Area} has no road next to it.");
            }
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_HasTheTownsLandmarksAndCountryside(int seed)
        {
            var uses = new HashSet<PlotUse>(Generate(seed).Layout.Plots.Select(plot => plot.Use));

            PlotUse[] expected = { PlotUse.Water, PlotUse.Plaza, PlotUse.Fountain, PlotUse.Church, PlotUse.Market, PlotUse.Park, PlotUse.Garden, PlotUse.Yard, PlotUse.Field };
            CollectionAssert.IsSubsetOf(expected, uses);
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_BuildsSomeHousesWithoutNeighbours(int seed)
        {
            CityLayout layout = Generate(seed).Layout;
            List<GridRect> buildings = layout.Plots.Where(plot => plot.HasBuilding).Select(plot => plot.Area).ToList();

            bool anyDetached = buildings.Any(building => !buildings.Any(other => !other.Equals(building) && Touch(building, other)));
            Assert.IsTrue(anyDetached);
        }

        [Test]
        public void Generate_PutsTheDefaultHillOnARoad()
        {
            HillSettings hill = HillSettings.Default;

            Assert.IsTrue(Generate(0).Layout.IsRoadAt(hill.CenterXMetres, hill.CenterZMetres));
        }

        [Test]
        public void Validate_RejectsATownTooSmallForItsCountryside()
        {
            var settings = new CanalTownSettings { WidthCells = 60, CountrysideCells = 20 };

            Assert.Throws<System.ArgumentException>(settings.Validate);
        }

        private static IEnumerable<GridPosition> Neighbours(GridRect area)
        {
            for (int x = area.Min.X; x < area.XMax; x++)
            {
                yield return new GridPosition(x, area.Min.Y - 1);
                yield return new GridPosition(x, area.YMax);
            }

            for (int y = area.Min.Y; y < area.YMax; y++)
            {
                yield return new GridPosition(area.Min.X - 1, y);
                yield return new GridPosition(area.XMax, y);
            }
        }

        private static bool Touch(GridRect first, GridRect second)
        {
            bool overlapX = first.Min.X <= second.XMax && second.Min.X <= first.XMax;
            bool overlapY = first.Min.Y <= second.YMax && second.Min.Y <= first.YMax;
            return overlapX && overlapY;
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_PutsEveryBridgeAcrossTheCanalOnARoad(int seed)
        {
            Stilargo.Core.City.CanalTown.CanalTown town = Generate(seed);
            Canal canal = town.Canal;

            Assert.IsNotEmpty(canal.Bridges);
            foreach (int bridgeX in canal.Bridges)
            {
                var deck = new GridRect(new GridPosition(bridgeX, canal.Water.Min.Y), canal.BridgeWidthCells, canal.Water.Height);
                Assert.IsTrue(deck.Cells().All(town.Layout.IsRoad), $"Bridge at x {bridgeX} is not a road across the water.");
            }
        }

        [TestCaseSource(nameof(Seeds))]
        public void Generate_StylesAndFacesEveryHouseTowardsAStreetOrSquare(int seed)
        {
            CityLayout layout = Generate(seed).Layout;
            List<Plot> houses = layout.Plots.Where(plot => plot.HasBuilding).ToList();

            Assert.IsTrue(houses.Any(plot => plot.Style == HouseStyle.Row), "No row houses.");
            Assert.IsTrue(houses.Any(plot => plot.Style == HouseStyle.Corner), "No corner houses.");
            Assert.IsTrue(houses.Any(plot => plot.Style == HouseStyle.Farmhouse), "No farmhouses.");
            Assert.IsTrue(houses.Any(plot => plot.Front != Facing.South), "Every house faces south.");
        }
    }
}
