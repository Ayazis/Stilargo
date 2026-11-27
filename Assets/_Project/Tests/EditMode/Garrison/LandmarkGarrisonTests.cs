using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Garrison;

namespace Stilargo.Tests.EditMode.Garrison
{
    public sealed class LandmarkGarrisonTests
    {
        private const float CellSize = 2f;
        private const int GridSize = 8;
        private const int TowerStoreys = 6;

        private static CityLayout CreateCity(Plot plot)
        {
            var grid = new CityGrid(GridSize, GridSize, CellSize);
            for (int x = 0; x < GridSize; x++)
            {
                grid.MarkRoad(new GridPosition(x, 0));
            }

            return new CityLayout(grid, new List<GridRect>(), new List<Plot> { plot });
        }

        private static Plot StreetPlot()
        {
            return new Plot(new GridRect(new GridPosition(1, 1), 4, 4), 1);
        }

        [Test]
        public void Park_GetsNoGarrisonBuilding()
        {
            Plot park = StreetPlot().UsedFor(new PlotUseRule(PlotUse.Park, 1, PlotPlacement.Anywhere));

            IReadOnlyList<GarrisonBuilding> buildings = new GarrisonSlotGenerator(new GarrisonLayoutSettings()).Generate(CreateCity(park));

            Assert.IsEmpty(buildings);
        }

        [Test]
        public void Church_PutsItsRoofSlotsOnTheTowerTop()
        {
            Plot church = StreetPlot().UsedFor(new PlotUseRule(PlotUse.Church, 1, PlotPlacement.Largest).WithHeight(2, TowerStoreys));
            var settings = new GarrisonLayoutSettings();

            GarrisonBuilding building = new GarrisonSlotGenerator(settings).Generate(CreateCity(church)).Single();
            List<GarrisonSlot> roofSlots = building.Slots.Where(slot => slot.Kind == GarrisonSlotKind.Roof).ToList();

            Assert.IsNotEmpty(roofSlots);
            GridRect tower = BellTower.AreaOf(church);
            foreach (GarrisonSlot slot in roofSlots)
            {
                Assert.AreEqual(TowerStoreys * settings.StoreyHeightMetres, slot.Position.Y, 0.001f);
                Assert.IsTrue(tower.Contains(new GridPosition((int)(slot.Position.X / CellSize), (int)(slot.Position.Z / CellSize))));
            }
        }
    }
}
