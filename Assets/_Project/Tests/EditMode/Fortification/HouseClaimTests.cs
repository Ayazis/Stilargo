using System;
using NUnit.Framework;
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
        public void TryClaim_Second_IsRefusedAndKeepsFirstHouse()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());
            claim.TryClaim(FortificationTestHouse.CreateBuilding());
            ClaimedHouse first = claim.House;
            bool raisedAgain = false;
            claim.Claimed += _ => raisedAgain = true;

            Assert.IsFalse(claim.TryClaim(FortificationTestHouse.CreateBuilding()));
            Assert.AreSame(first, claim.House);
            Assert.IsFalse(raisedAgain);
        }

        [Test]
        public void TryClaim_Null_Throws()
        {
            HouseClaim claim = FortificationTestHouse.CreateClaim(FortificationTestHouse.CreateDropOff());

            Assert.Throws<ArgumentNullException>(() => claim.TryClaim(null));
        }
    }
}
