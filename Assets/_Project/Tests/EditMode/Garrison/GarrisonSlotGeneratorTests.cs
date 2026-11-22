using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Garrison;

namespace Stilargo.Tests.EditMode.Garrison
{
    public sealed class GarrisonSlotGeneratorTests
    {
        private const float Tolerance = 1e-4f;
        private const float StoreyHeight = 3f;
        private const float WindowOffset = 0.5f;
        private const int RoofSlots = 4;
        private const int WindowsPerFacade = 2;

        // The street plot spans 2-6 m on X and Z; its road-facing side is the south wall at Z = 2 m.
        private const float SouthWallZ = 2f;
        private const float PlotWestX = 2f;
        private const float PlotEastX = 6f;
        private const float PlotNorthZ = 6f;

        [Test]
        public void Generate_PlotWithoutRoad_IsSkipped()
        {
            IReadOnlyList<GarrisonBuilding> buildings = Generate();

            Assert.AreEqual(1, buildings.Count);
            Assert.AreSame(GarrisonTestCity.StreetPlot, buildings[0].Plot);
        }

        [Test]
        public void Generate_WindowsOnlyOnRoadFacingFacade_PerStorey()
        {
            GarrisonBuilding building = Generate()[0];
            List<GarrisonSlot> windows = building.Slots.Where(slot => slot.Kind == GarrisonSlotKind.Window).ToList();

            Assert.AreEqual(GarrisonTestCity.StreetPlotStoreys * WindowsPerFacade, windows.Count);
            for (int storey = 0; storey < GarrisonTestCity.StreetPlotStoreys; storey++)
            {
                Assert.AreEqual(WindowsPerFacade, windows.Count(slot => slot.Storey == storey));
            }

            foreach (GarrisonSlot window in windows)
            {
                Assert.AreEqual(-Vector3.UnitZ, window.Facing);
                Assert.AreEqual(SouthWallZ - WindowOffset, window.Position.Z, Tolerance);
                Assert.AreEqual(window.Storey * StoreyHeight, window.Position.Y, Tolerance);
                Assert.That(window.Position.X, Is.InRange(PlotWestX, PlotEastX));
            }
        }

        [Test]
        public void Generate_RoofSlots_SitOnTopInsideFootprint()
        {
            GarrisonBuilding building = Generate()[0];
            List<GarrisonSlot> roofs = building.Slots.Where(slot => slot.Kind == GarrisonSlotKind.Roof).ToList();
            float roofHeight = GarrisonTestCity.StreetPlotStoreys * StoreyHeight;

            Assert.AreEqual(RoofSlots, roofs.Count);
            Assert.AreEqual(RoofSlots, roofs.Select(slot => slot.Position).Distinct().Count());
            foreach (GarrisonSlot roof in roofs)
            {
                Assert.AreEqual(GarrisonTestCity.StreetPlotStoreys, roof.Storey);
                Assert.AreEqual(roofHeight, roof.Position.Y, Tolerance);
                Assert.That(roof.Position.X, Is.InRange(PlotWestX, PlotEastX));
                Assert.That(roof.Position.Z, Is.InRange(SouthWallZ, PlotNorthZ));
                Assert.AreEqual(-Vector3.UnitZ, roof.Facing);
            }
        }

        [Test]
        public void Generate_RoofSlotsAreListedFirst()
        {
            GarrisonBuilding building = Generate()[0];

            Assert.AreEqual(GarrisonSlotKind.Roof, building.Slots[0].Kind);
        }

        [Test]
        public void Generate_Entrance_IsOnAdjacentRoadCellCentre()
        {
            CityLayout layout = GarrisonTestCity.Create();
            GarrisonBuilding building = new GarrisonSlotGenerator(CreateSettings()).Generate(layout)[0];

            Assert.IsTrue(layout.IsRoadAt(building.Entrance.X, building.Entrance.Z));
            Assert.AreEqual(0f, building.Entrance.Y);
            Assert.AreEqual(new Vector3(3f, 0f, 1f), building.Entrance);
        }

        [Test]
        public void Settings_NegativeRoofSlotCount_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GarrisonLayoutSettings().WithRoofSlotCount(-1));
        }

        [Test]
        public void Settings_NonPositiveStoreyHeight_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new GarrisonLayoutSettings(0f, WindowOffset));
        }

        private static IReadOnlyList<GarrisonBuilding> Generate()
        {
            return new GarrisonSlotGenerator(CreateSettings()).Generate(GarrisonTestCity.Create());
        }

        private static GarrisonLayoutSettings CreateSettings()
        {
            return new GarrisonLayoutSettings(StoreyHeight, WindowOffset)
                .WithRoofSlotCount(RoofSlots)
                .WithWindowsPerFacadePerStorey(WindowsPerFacade);
        }
    }
}
