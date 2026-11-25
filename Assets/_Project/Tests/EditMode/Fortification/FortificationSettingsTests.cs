using System;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;

namespace Stilargo.Tests.EditMode.Fortification
{
    public sealed class FortificationSettingsTests
    {
        [Test]
        public void Default_HasBarricadeWindowAndTurret()
        {
            FortificationSettings settings = FortificationSettings.Default;

            Assert.AreEqual(FortificationSlotKind.Entrance, FortificationTestHouse.Find("barricade").SlotKind);
            Assert.AreEqual(FortificationSlotKind.Window, FortificationTestHouse.Find("reinforcedWindow").SlotKind);
            Assert.IsTrue(FortificationTestHouse.Find("turret").IsTurret);
            Assert.AreEqual(3, settings.Definitions.Count);
        }

        [Test]
        public void DuplicateIds_Throw()
        {
            FortificationDefinition[] definitions =
            {
                new FortificationDefinition("wall", FortificationSlotKind.Entrance, 10),
                new FortificationDefinition("wall", FortificationSlotKind.Window, 20),
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() => new FortificationSettings(100f, definitions));
            StringAssert.Contains("wall", exception.Message);
        }

        [TestCase(0f)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidBaseHealth_Throws(float baseHealth)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FortificationSettings(baseHealth, Array.Empty<FortificationDefinition>()));
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Definition_EmptyId_Throws(string id)
        {
            Assert.Throws<ArgumentException>(() => new FortificationDefinition(id, FortificationSlotKind.Roof, 10));
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Definition_NonPositiveCost_Throws(int cost)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FortificationDefinition("wall", FortificationSlotKind.Roof, cost));
        }

        [Test]
        public void Definition_NegativeHealthBonus_Throws()
        {
            FortificationDefinition definition = new FortificationDefinition("wall", FortificationSlotKind.Roof, 10);

            Assert.Throws<ArgumentOutOfRangeException>(() => definition.WithHealthBonus(-1f));
        }

        [Test]
        public void Definition_WithMethods_LeaveOriginalUnchanged()
        {
            FortificationDefinition original = new FortificationDefinition("gun", FortificationSlotKind.Roof, 10);

            FortificationDefinition turret = original.WithTurret(new WeaponSettings(1f, 5f, 1f));

            Assert.IsFalse(original.IsTurret);
            Assert.IsTrue(turret.IsTurret);
            Assert.AreEqual("gun", turret.DisplayName);
        }
    }
}
