using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Garrison;

namespace Stilargo.Tests.EditMode.Garrison
{
    public sealed class GarrisonBuildingTests
    {
        private const int RoofSlots = 1;
        private const int WindowsPerFacade = 1;

        [Test]
        public void TryOccupy_FreeBuilding_TakesRoofFirstAndRaisesEvent()
        {
            GarrisonBuilding building = CreateBuilding();
            object unit = new object();
            GarrisonSlot raised = null;
            building.SlotOccupied += slot => raised = slot;

            Assert.IsTrue(building.TryOccupy(unit, out GarrisonSlot taken));
            Assert.AreEqual(GarrisonSlotKind.Roof, taken.Kind);
            Assert.AreSame(unit, taken.Occupant);
            Assert.AreSame(taken, raised);
            Assert.IsTrue(building.Contains(unit));
        }

        [Test]
        public void TryOccupy_FullBuilding_ReturnsFalse()
        {
            GarrisonBuilding building = CreateBuilding();
            FillAll(building);

            Assert.IsFalse(building.HasFreeSlot);
            Assert.IsFalse(building.TryOccupy(new object(), out GarrisonSlot slot));
            Assert.IsNull(slot);
        }

        [Test]
        public void TryOccupy_SameOccupantTwice_Throws()
        {
            GarrisonBuilding building = CreateBuilding();
            object unit = new object();
            building.TryOccupy(unit, out _);

            Assert.Throws<InvalidOperationException>(() => building.TryOccupy(unit, out _));
        }

        [Test]
        public void Release_Occupant_FreesSlotAndRaisesEvent()
        {
            GarrisonBuilding building = CreateBuilding();
            object unit = new object();
            building.TryOccupy(unit, out GarrisonSlot taken);
            GarrisonSlot raised = null;
            building.SlotReleased += slot => raised = slot;

            Assert.IsTrue(building.Release(unit));
            Assert.IsFalse(taken.IsOccupied);
            Assert.AreSame(taken, raised);
            Assert.IsFalse(building.Contains(unit));
        }

        [Test]
        public void Release_Stranger_ReturnsFalse()
        {
            GarrisonBuilding building = CreateBuilding();

            Assert.IsFalse(building.Release(new object()));
        }

        [Test]
        public void TryOccupy_NullOccupant_Throws()
        {
            GarrisonBuilding building = CreateBuilding();

            Assert.Throws<ArgumentNullException>(() => building.TryOccupy(null, out _));
        }

        private static void FillAll(GarrisonBuilding building)
        {
            for (int index = 0; index < building.Slots.Count; index++)
            {
                building.TryOccupy(new object(), out _);
            }
        }

        private static GarrisonBuilding CreateBuilding()
        {
            GarrisonLayoutSettings settings = new GarrisonLayoutSettings()
                .WithRoofSlotCount(RoofSlots)
                .WithWindowsPerFacadePerStorey(WindowsPerFacade);
            IReadOnlyList<GarrisonBuilding> buildings = new GarrisonSlotGenerator(settings).Generate(GarrisonTestCity.Create());
            return buildings[0];
        }
    }
}
