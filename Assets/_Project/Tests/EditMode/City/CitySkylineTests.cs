using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class CitySkylineTests
    {
        private const float StoreyHeight = 3f;
        private const int GridSize = 12;

        private static readonly Plot House = new Plot(new GridRect(new GridPosition(0, 0), 3, 3), 2);
        private static readonly Plot Park = new Plot(new GridRect(new GridPosition(4, 0), 3, 3), 1)
            .UsedFor(new PlotUseRule(PlotUse.Park, 1, PlotPlacement.Anywhere));
        private static readonly Plot Church = new Plot(new GridRect(new GridPosition(0, 5), 4, 4), 1)
            .UsedFor(new PlotUseRule(PlotUse.Church, 1, PlotPlacement.Largest).WithHeight(2, 6));

        private static CitySkyline CreateSkyline()
        {
            var grid = new CityGrid(GridSize, GridSize, 2f);
            var layout = new CityLayout(grid, new List<GridRect>(), new List<Plot> { House, Park, Church });
            return new CitySkyline(layout, StoreyHeight);
        }

        [Test]
        public void Building_IsAsTallAsItsStoreys()
        {
            Assert.AreEqual(6f, CreateSkyline().ObstacleHeightAt(1, 1), 0.001f);
        }

        [Test]
        public void ParkAndStreet_AreOpen()
        {
            CitySkyline skyline = CreateSkyline();

            Assert.AreEqual(0f, skyline.ObstacleHeightAt(5, 1), 0.001f);
            Assert.AreEqual(0f, skyline.ObstacleHeightAt(10, 10), 0.001f);
        }

        [Test]
        public void BellTower_RisesAboveTheChurch()
        {
            CitySkyline skyline = CreateSkyline();
            GridRect tower = BellTower.AreaOf(Church);

            Assert.AreEqual(18f, skyline.ObstacleHeightAt(tower.Min.X, tower.Min.Y), 0.001f);
            Assert.AreEqual(6f, skyline.ObstacleHeightAt(Church.Area.XMax - 1, Church.Area.YMax - 1), 0.001f);
        }

        [Test]
        public void OutsideTheGrid_IsOpen()
        {
            Assert.AreEqual(0f, CreateSkyline().ObstacleHeightAt(-1, 3), 0.001f);
        }
    }
}
