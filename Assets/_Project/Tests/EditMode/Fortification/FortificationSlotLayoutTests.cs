using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Tests.EditMode.Garrison;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class FortificationSlotLayoutTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void Create_HasOneEntranceSlotAtTheBuildingEntrance()
        {
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();

            IReadOnlyList<FortificationSlot> slots = new FortificationSlotLayout(GarrisonTestCity.CellSize).Create(building);

            FortificationSlot entrance = slots.Single(slot => slot.Kind == FortificationSlotKind.Entrance);
            Assert.AreEqual(building.Entrance, entrance.Position);
        }

        [Test]
        public void Create_EntranceFacesTheStreet()
        {
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();

            FortificationSlot entrance = new FortificationSlotLayout(GarrisonTestCity.CellSize).Create(building)
                .Single(slot => slot.Kind == FortificationSlotKind.Entrance);

            // The street plot touches the road on its south side only.
            Assert.AreEqual(-1f, entrance.Facing.Z, Tolerance);
            Assert.AreEqual(0f, entrance.Facing.Y, Tolerance);
        }

        [Test]
        public void Create_MirrorsGarrisonWindowAndRoofSlots()
        {
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();

            IReadOnlyList<FortificationSlot> slots = new FortificationSlotLayout(GarrisonTestCity.CellSize).Create(building);

            Assert.AreEqual(Count(building, GarrisonSlotKind.Roof), slots.Count(slot => slot.Kind == FortificationSlotKind.Roof));
            Assert.AreEqual(Count(building, GarrisonSlotKind.Window), slots.Count(slot => slot.Kind == FortificationSlotKind.Window));
            CollectionAssert.IsSubsetOf(
                building.Slots.Select(slot => slot.Position).ToList(),
                slots.Select(slot => slot.Position).ToList());
        }

        [Test]
        public void Create_SlotsStartEmpty()
        {
            IReadOnlyList<FortificationSlot> slots = new FortificationSlotLayout(GarrisonTestCity.CellSize).Create(FortificationTestHouse.CreateBuilding());

            Assert.IsTrue(slots.All(slot => !slot.IsFilled));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_InvalidCellSize_Throws(float cellSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FortificationSlotLayout(cellSize));
        }

        [Test]
        public void Slot_ZeroFacing_Throws()
        {
            Assert.Throws<ArgumentException>(() => new FortificationSlot(FortificationSlotKind.Roof, Vector3.Zero, Vector3.Zero));
        }

        private static int Count(GarrisonBuilding building, GarrisonSlotKind kind)
        {
            return building.Slots.Count(slot => slot.Kind == kind);
        }
    }
}
