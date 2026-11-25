using System;
using NUnit.Framework;
using Stilargo.Core.Fortification;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class ClaimedHouseTests
    {
        private const float BaseHealth = 100f;

        [Test]
        public void TakeDamage_ReducesHealth()
        {
            ClaimedHouse house = CreateHouse();

            house.TakeDamage(25f);

            Assert.AreEqual(75f, house.CurrentHealth);
            Assert.AreEqual(0.75f, house.HealthFraction);
        }

        [Test]
        public void TakeDamage_Lethal_DiesOnceAndClampsToZero()
        {
            ClaimedHouse house = CreateHouse();
            int deaths = 0;
            house.Died += () => deaths++;

            house.TakeDamage(BaseHealth * 2f);
            house.TakeDamage(1f);

            Assert.IsFalse(house.IsAlive);
            Assert.AreEqual(0f, house.CurrentHealth);
            Assert.AreEqual(1, deaths);
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void TakeDamage_Invalid_Throws(float amount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CreateHouse().TakeDamage(amount));
        }

        [Test]
        public void Position_IsTheEntrance()
        {
            ClaimedHouse house = CreateHouse();

            Assert.AreEqual(house.Building.Entrance, house.Position);
        }

        [TestCase(0f)]
        [TestCase(float.NaN)]
        public void Constructor_InvalidBaseHealth_Throws(float baseHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ClaimedHouse(FortificationTestHouse.CreateBuilding(), Array.Empty<FortificationSlot>(), baseHealth));
        }

        private static ClaimedHouse CreateHouse()
        {
            return new ClaimedHouse(FortificationTestHouse.CreateBuilding(), Array.Empty<FortificationSlot>(), BaseHealth);
        }
    }
}
