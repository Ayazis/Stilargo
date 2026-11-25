using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Tests.EditMode.Garrison;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class ClaimCandidatePickerTests
    {
        [Test]
        public void TryPick_NoOneGarrisoned_FindsNothing()
        {
            ClaimCandidatePicker picker = new ClaimCandidatePicker(CreateBuildings(2));

            Assert.IsFalse(picker.TryPick(out GarrisonBuilding building));
            Assert.IsNull(building);
        }

        [Test]
        public void TryPick_PicksBuildingWithMostOccupants()
        {
            IReadOnlyList<GarrisonBuilding> buildings = CreateBuildings(2);
            buildings[0].TryOccupy(new object(), out _);
            buildings[1].TryOccupy(new object(), out _);
            buildings[1].TryOccupy(new object(), out _);

            new ClaimCandidatePicker(buildings).TryPick(out GarrisonBuilding building);

            Assert.AreSame(buildings[1], building);
        }

        [Test]
        public void TryPick_Tie_PicksFirstListed()
        {
            IReadOnlyList<GarrisonBuilding> buildings = CreateBuildings(2);
            buildings[0].TryOccupy(new object(), out _);
            buildings[1].TryOccupy(new object(), out _);

            new ClaimCandidatePicker(buildings).TryPick(out GarrisonBuilding building);

            Assert.AreSame(buildings[0], building);
        }

        private static IReadOnlyList<GarrisonBuilding> CreateBuildings(int count)
        {
            return Enumerable.Range(0, count).Select(_ => FortificationTestHouse.CreateBuilding()).ToList();
        }
    }
}
