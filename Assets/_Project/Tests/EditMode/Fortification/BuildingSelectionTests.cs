using System.Collections.Generic;
using NUnit.Framework;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Hud;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class BuildingSelectionTests
    {
        private GarrisonBuilding _garrisoned;
        private GarrisonBuilding _clicked;
        private HouseClaim _claim;
        private BuildingSelection _selection;
        private ClaimTarget _target;

        [SetUp]
        public void SetUp()
        {
            _claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            _garrisoned = FortificationTestHouse.CreateBuilding();
            _clicked = FortificationTestHouse.CreateBuilding();
            _selection = new BuildingSelection();
            var picker = new ClaimCandidatePicker(new List<GarrisonBuilding> { _garrisoned, _clicked });
            _target = new ClaimTarget(_claim, picker, _selection);
            _garrisoned.TryOccupy(new object(), out _);
        }

        [Test]
        public void FocusedBuilding_WithoutAClick_IsTheGarrisonedOne()
        {
            Assert.AreSame(_garrisoned, _target.FocusedBuilding);
        }

        [Test]
        public void FocusedBuilding_AfterAClick_IsTheClickedOne()
        {
            _selection.Select(_clicked);

            Assert.AreSame(_clicked, _target.FocusedBuilding);
        }

        [Test]
        public void CanClaim_ClickedBuildingWithoutUnits_IsFalse()
        {
            _selection.Select(_clicked);

            Assert.IsFalse(_target.CanClaim);
            Assert.IsFalse(_target.TryClaim());
        }

        [Test]
        public void TryClaim_ClickedBuildingWithUnits_ClaimsIt()
        {
            _clicked.TryOccupy(new object(), out _);
            _selection.Select(_clicked);

            Assert.IsTrue(_target.TryClaim());
            Assert.IsTrue(_claim.TryGetHouse(_clicked, out _));
        }

        [Test]
        public void SelectedHouse_ClickedBuildingThatIsNotYours_IsNone()
        {
            Assert.IsTrue(_target.TryClaim());
            _selection.Select(_clicked);

            Assert.IsNull(_target.SelectedHouse);
        }

        [Test]
        public void SelectedHouse_AfterClearing_FallsBackToTheGarrisonedHouse()
        {
            Assert.IsTrue(_target.TryClaim());
            _selection.Select(_clicked);

            _selection.Clear();

            Assert.AreSame(_claim.House, _target.SelectedHouse);
        }

        [Test]
        public void Select_SameBuildingTwice_RaisesChangedOnce()
        {
            int changes = 0;
            _selection.Changed += () => changes++;

            _selection.Select(_clicked);
            _selection.Select(_clicked);

            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Describe_UnclaimedBuilding_SaysUnoccupiedAndGarrison()
        {
            _selection.Select(_clicked);

            string info = BuildingInfo.Describe(_target);

            StringAssert.Contains(BuildingInfo.Unoccupied, info);
            StringAssert.Contains($"0/{_clicked.Slots.Count} garrisoned", info);
        }

        [Test]
        public void Describe_OwnHouse_SaysYoursWithHealth()
        {
            Assert.IsTrue(_target.TryClaim());

            string info = BuildingInfo.Describe(_target);

            StringAssert.Contains(BuildingInfo.Yours, info);
            StringAssert.Contains("HP", info);
        }

        [Test]
        public void Describe_NothingFocused_IsEmpty()
        {
            _garrisoned.Release(_garrisoned.Slots[0].Occupant);

            Assert.AreEqual(string.Empty, BuildingInfo.Describe(_target));
        }
    }
}
