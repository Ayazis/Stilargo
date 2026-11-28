using System;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class ShowcaseHouseTests
    {
        private const int GridSize = 40;
        private const float CellSize = 2f;

        [Test]
        public void Place_TakesTheSouthernmostField()
        {
            Plot north = Open(PlotUse.Field, 15, 30);
            Plot south = Open(PlotUse.Meadow, 15, 2);

            ShowcaseSite site = ShowcaseHouse.Place(Layout(north, south));

            Assert.AreSame(south, site.Replaced);
        }

        [Test]
        public void Place_PrefersTheMiddleAmongEquallySouthernFields()
        {
            Plot edge = Open(PlotUse.Field, 0, 2);
            Plot middle = Open(PlotUse.Field, 15, 2);

            Assert.AreSame(middle, ShowcaseHouse.Place(Layout(edge, middle)).Replaced);
        }

        [Test]
        public void Place_HouseStandsInsideTheSiteAndTheMap()
        {
            ShowcaseSite site = ShowcaseHouse.Place(Layout(Open(PlotUse.Field, 15, 2)));

            GridRect house = site.House.Area;
            GridRect field = site.Replaced.Area;
            Assert.That(house.Min.X >= field.Min.X && house.XMax <= field.XMax && house.Min.Y >= field.Min.Y && house.YMax <= field.YMax);
            Assert.That(house.Min.Y, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void Place_IgnoresBuildingsAndSmallPlots()
        {
            Plot house = new Plot(new GridRect(new GridPosition(15, 0), 10, 10), 2, PlotUse.House);
            Plot tiny = new Plot(new GridRect(new GridPosition(5, 0), 3, 3), 1, PlotUse.Field);
            Plot field = Open(PlotUse.Field, 15, 20);

            Assert.AreSame(field, ShowcaseHouse.Place(Layout(house, tiny, field)).Replaced);
        }

        [Test]
        public void Place_HouseIsAHouse()
        {
            Assert.AreEqual(PlotUse.House, ShowcaseHouse.Place(Layout(Open(PlotUse.Field, 15, 2))).House.Use);
        }

        [Test]
        public void Place_NoOpenPlot_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => ShowcaseHouse.Place(Layout()));
        }

        [Test]
        public void Place_NullLayout_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ShowcaseHouse.Place(null));
        }

        private static Plot Open(PlotUse use, int x, int y)
        {
            return new Plot(new GridRect(new GridPosition(x, y), 10, 8), 1, use);
        }

        private static CityLayout Layout(params Plot[] plots)
        {
            return new CityLayout(new CityGrid(GridSize, GridSize, CellSize), Array.Empty<GridRect>(), plots);
        }
    }
}
