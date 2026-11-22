using NUnit.Framework;
using Stilargo.Core.Garrison;

namespace Stilargo.Tests.EditMode.Garrison
{
    public sealed class GarrisonDirectoryTests
    {
        private const float InsideStreetPlot = 4f;
        private const float OnRoad = 1f;
        private const float InsideBackPlotZ = 9f;

        [Test]
        public void TryFindBuildingAt_PointInsideFootprint_FindsBuilding()
        {
            GarrisonDirectory directory = CreateDirectory();

            Assert.IsTrue(directory.TryFindBuildingAt(InsideStreetPlot, InsideStreetPlot, out GarrisonBuilding building));
            Assert.AreSame(GarrisonTestCity.StreetPlot, building.Plot);
        }

        [Test]
        public void TryFindBuildingAt_PointOnRoad_FindsNothing()
        {
            GarrisonDirectory directory = CreateDirectory();

            Assert.IsFalse(directory.TryFindBuildingAt(InsideStreetPlot, OnRoad, out GarrisonBuilding building));
            Assert.IsNull(building);
        }

        [Test]
        public void TryFindBuildingAt_PlotWithoutRoad_FindsNothing()
        {
            GarrisonDirectory directory = CreateDirectory();

            Assert.IsFalse(directory.TryFindBuildingAt(InsideStreetPlot, InsideBackPlotZ, out _));
        }

        [Test]
        public void TryFindOccupiedBuilding_AfterOccupy_FindsIt()
        {
            GarrisonDirectory directory = CreateDirectory();
            object unit = new object();
            directory.Buildings[0].TryOccupy(unit, out _);

            Assert.IsTrue(directory.TryFindOccupiedBuilding(unit, out GarrisonBuilding building));
            Assert.AreSame(directory.Buildings[0], building);
        }

        [Test]
        public void ReleaseEverywhere_Occupant_ReleasesAndReportsIt()
        {
            GarrisonDirectory directory = CreateDirectory();
            object unit = new object();
            directory.Buildings[0].TryOccupy(unit, out _);

            Assert.IsTrue(directory.ReleaseEverywhere(unit));
            Assert.IsFalse(directory.TryFindOccupiedBuilding(unit, out _));
            Assert.IsFalse(directory.ReleaseEverywhere(unit));
        }

        private static GarrisonDirectory CreateDirectory()
        {
            GarrisonSlotGenerator generator = new GarrisonSlotGenerator(new GarrisonLayoutSettings());
            return new GarrisonDirectory(generator.Generate(GarrisonTestCity.Create()), GarrisonTestCity.CellSize);
        }
    }
}
