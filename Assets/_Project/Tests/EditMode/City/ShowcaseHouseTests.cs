using System;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class ShowcaseHouseTests
    {
        private const int GridSize = 20;
        private const float CellSize = 2f;

        [Test]
        public void BesideCity_StandsSouthOfTheGrid()
        {
            Plot house = ShowcaseHouse.BesideCity(EmptyLayout());

            Assert.That(house.Area.YMax, Is.LessThan(0));
        }

        [Test]
        public void BesideCity_IsCentredAlongTheCity()
        {
            Plot house = ShowcaseHouse.BesideCity(EmptyLayout());

            Assert.AreEqual(GridSize, house.Area.Min.X + house.Area.XMax);
        }

        [Test]
        public void BesideCity_IsAHouse()
        {
            Assert.AreEqual(PlotUse.House, ShowcaseHouse.BesideCity(EmptyLayout()).Use);
        }

        [Test]
        public void BesideCity_NullLayout_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ShowcaseHouse.BesideCity(null));
        }

        private static CityLayout EmptyLayout()
        {
            return new CityLayout(new CityGrid(GridSize, GridSize, CellSize), Array.Empty<GridRect>(), Array.Empty<Plot>());
        }
    }
}
