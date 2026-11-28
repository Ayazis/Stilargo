using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class HouseStylerTests
    {
        private const int GridSize = 12;
        private const float CellSize = 2f;
        private const int Side = 3;

        private static GridRect Square(int x, int y)
        {
            return new GridRect(new GridPosition(x, y), Side, Side);
        }

        // A grid with a street along the bottom row (y = 0) and, if asked, the left column (x = 0).
        private static CityLayout Layout(IReadOnlyList<Plot> plots, bool westStreet = false)
        {
            var grid = new CityGrid(GridSize, GridSize, CellSize);
            for (int x = 0; x < GridSize; x++)
            {
                grid.MarkRoad(new GridPosition(x, 0));
            }

            for (int y = 0; westStreet && y < GridSize; y++)
            {
                grid.MarkRoad(new GridPosition(0, y));
            }

            return new CityLayout(grid, new List<GridRect>(), plots);
        }

        private static Plot Styled(CityLayout layout, GridRect area)
        {
            return HouseStyler.Style(layout).Plots.Single(plot => plot.Area.Equals(area));
        }

        [Test]
        public void Style_MakesAHouseBetweenNeighboursARowHouseFacingTheStreet()
        {
            var plots = new List<Plot> { new Plot(Square(1, 1), 2), new Plot(Square(4, 1), 2), new Plot(Square(7, 1), 2) };

            Plot middle = Styled(Layout(plots), Square(4, 1));

            Assert.AreEqual(HouseStyle.Row, middle.Style);
            Assert.AreEqual(Facing.South, middle.Front);
        }

        [Test]
        public void Style_MakesAHouseWithStreetOnTwoSidesACornerHouse()
        {
            var plots = new List<Plot> { new Plot(Square(1, 1), 3), new Plot(Square(4, 1), 2) };

            Plot corner = Styled(Layout(plots, westStreet: true), Square(1, 1));

            Assert.AreEqual(HouseStyle.Corner, corner.Style);
            Assert.AreEqual(Facing.South, corner.Front);
        }

        [Test]
        public void Style_TurnsAHouseOnASquareToFaceTheSquare()
        {
            var plots = new List<Plot>
            {
                new Plot(Square(1, 1), 3),
                new Plot(Square(4, 1), 3),
                new Plot(Square(1, 4), 1, PlotUse.Plaza),
            };

            Plot townhouse = Styled(Layout(plots), Square(1, 1));

            Assert.AreEqual(HouseStyle.Townhouse, townhouse.Style);
            Assert.AreEqual(Facing.North, townhouse.Front);
        }

        [Test]
        public void Style_MakesAFreeStandingHouseDetached()
        {
            var plots = new List<Plot> { new Plot(Square(4, 1), 2), new Plot(Square(4, 4), 1, PlotUse.Garden) };

            Plot home = Styled(Layout(plots), Square(4, 1));

            Assert.AreEqual(HouseStyle.Detached, home.Style);
            Assert.AreEqual(Facing.South, home.Front);
        }

        [Test]
        public void Style_MakesALoneHouseOnAYardAFarmhouse()
        {
            var plots = new List<Plot> { new Plot(Square(4, 1), 1), new Plot(Square(4, 4), 1, PlotUse.Yard) };

            Assert.AreEqual(HouseStyle.Farmhouse, Styled(Layout(plots), Square(4, 1)).Style);
        }

        [Test]
        public void Style_MakesAHouseInARowOnAYardAWorkshop()
        {
            var plots = new List<Plot>
            {
                new Plot(Square(4, 1), 1),
                new Plot(Square(7, 1), 1),
                new Plot(Square(4, 4), 1, PlotUse.Yard),
            };

            Assert.AreEqual(HouseStyle.Workshop, Styled(Layout(plots), Square(4, 1)).Style);
        }

        [Test]
        public void Style_FacesAHouseTheSideWithTheMostStreet()
        {
            var plots = new List<Plot> { new Plot(new GridRect(new GridPosition(1, 2), 2, 4), 2) };

            Plot house = Styled(Layout(plots, westStreet: true), new GridRect(new GridPosition(1, 2), 2, 4));

            Assert.AreEqual(Facing.West, house.Front);
        }

        [Test]
        public void Style_LeavesOpenGroundAlone()
        {
            var garden = new Plot(Square(4, 4), 1, PlotUse.Garden);

            CityLayout styled = HouseStyler.Style(Layout(new List<Plot> { garden }));

            Assert.AreSame(garden, styled.Plots.Single());
        }

        [Test]
        public void UsedFor_KeepsTheStyleAndFront()
        {
            Plot house = new Plot(Square(1, 1), 2).Styled(HouseStyle.Corner, Facing.East);

            Plot tavern = house.UsedFor(new PlotUseRule(PlotUse.Tavern, 1, PlotPlacement.Anywhere));

            Assert.AreEqual(HouseStyle.Corner, tavern.Style);
            Assert.AreEqual(Facing.East, tavern.Front);
        }
    }
}
