using System;
using NUnit.Framework;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Core.Garrison;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class HouseClaimTests
    {
        [Test]
        public void NewClaim_HasNoHouse()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());

            Assert.IsFalse(claim.IsClaimed);
            Assert.IsNull(claim.House);
        }

        [Test]
        public void TryClaim_First_ClaimsBuildingAndRaisesEvent()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();
            ClaimedHouse reported = null;
            claim.Claimed += house => reported = house;

            Assert.IsTrue(claim.TryClaim(building));
            Assert.AreSame(building, claim.House.Building);
            Assert.AreSame(claim.House, reported);
        }

        [Test]
        public void TryClaim_HouseStartsWithBaseHealth()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());

            claim.TryClaim(FortificationTestHouse.CreateBuilding());

            Assert.AreEqual(FortificationSettings.DefaultHouseBaseHealthPoints, claim.House.MaxHealth);
            Assert.AreEqual(1f, claim.House.HealthFraction);
        }

        [Test]
        public void TryClaim_MovesDropOffToEntrance()
        {
            DropOff dropOff = FortificationTestHouse.CreateDropOff();
            HouseClaim claim = FortificationTestHouse.CreateClaim(dropOff);
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();

            claim.TryClaim(building);

            Assert.AreEqual(building.Entrance.X, dropOff.Position.X);
            Assert.AreEqual(building.Entrance.Z, dropOff.Position.Z);
        }

        [Test]
        public void TryClaim_SameBuildingTwice_IsRefused()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            GarrisonBuilding building = FortificationTestHouse.CreateBuilding();
            claim.TryClaim(building);
            bool raisedAgain = false;
            claim.Claimed += _ => raisedAgain = true;

            Assert.IsFalse(claim.TryClaim(building));
            Assert.AreEqual(1, claim.Houses.Count);
            Assert.IsFalse(raisedAgain);
        }

        [Test]
        public void TryClaim_OtherBuilding_AddsHouseAndKeepsFirstAsBase()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            claim.TryClaim(FortificationTestHouse.CreateBuilding());
            ClaimedHouse first = claim.House;
            ClaimedHouse reported = null;
            claim.Claimed += house => reported = house;
            GarrisonBuilding second = FortificationTestHouse.CreateBuilding();

            Assert.IsTrue(claim.TryClaim(second));
            Assert.AreSame(first, claim.House);
            Assert.AreEqual(2, claim.Houses.Count);
            Assert.AreSame(second, reported.Building);
            Assert.IsTrue(claim.TryGetHouse(second, out ClaimedHouse found));
            Assert.AreSame(reported, found);
        }

        [Test]
        public void TryClaim_OtherBuilding_LeavesDropOffAtBase()
        {
            DropOff dropOff = FortificationTestHouse.CreateDropOff();
            HouseClaim claim = FortificationTestHouse.CreateClaim(dropOff);
            claim.TryClaim(FortificationTestHouse.CreateBuilding());
            var marker = new WorldPoint(-5f, -5f);
            dropOff.Relocate(marker);

            claim.TryClaim(FortificationTestHouse.CreateBuilding());

            Assert.AreEqual(marker.X, dropOff.Position.X);
            Assert.AreEqual(marker.Z, dropOff.Position.Z);
        }

        [Test]
        public void TryClaim_Null_Throws()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());

            Assert.Throws<ArgumentNullException>(() => claim.TryClaim(null));
        }
    }
}
