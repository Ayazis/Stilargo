using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Garrison;

namespace Stilargo.Tests.EditMode.Garrison
{
    public sealed class GarrisonBonusesTests
    {
        [Test]
        public void For_Window_ReturnsWindowDefaults()
        {
            PositionBonus bonus = new GarrisonBonuses().For(GarrisonSlotKind.Window);

            Assert.AreEqual(GarrisonBonuses.DefaultWindowRangeMetres, bonus.RangeMetres);
            Assert.AreEqual(GarrisonBonuses.DefaultWindowSightMetres, bonus.SightMetres);
            Assert.AreEqual(GarrisonBonuses.DefaultWindowDamageMultiplier, bonus.Protection.DamageMultiplier);
            Assert.AreEqual(1f, bonus.Protection.HitChanceMultiplier);
        }

        [Test]
        public void For_Roof_ReturnsRoofDefaults()
        {
            PositionBonus bonus = new GarrisonBonuses().For(GarrisonSlotKind.Roof);

            Assert.AreEqual(GarrisonBonuses.DefaultRoofRangeMetres, bonus.RangeMetres);
            Assert.AreEqual(GarrisonBonuses.DefaultRoofSightMetres, bonus.SightMetres);
            Assert.AreEqual(GarrisonBonuses.DefaultRoofDamageMultiplier, bonus.Protection.DamageMultiplier);
        }

        [Test]
        public void For_Slot_UsesItsKind()
        {
            GarrisonSlot slot = new GarrisonSlot(GarrisonSlotKind.Roof, 1, new GarrisonSlotPose(Vector3.Zero, Vector3.UnitX));

            Assert.AreEqual(GarrisonBonuses.DefaultRoofRangeMetres, new GarrisonBonuses().For(slot).RangeMetres);
        }
    }
}
