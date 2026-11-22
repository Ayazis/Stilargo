using System;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.Combat;
using Stilargo.Core.Cover;

namespace Stilargo.Tests.EditMode.Cover
{
    public sealed class CoverSettingsTests
    {
        [Test]
        public void Default_HasDocumentedValues()
        {
            CoverSettings settings = CoverSettings.Default;

            Assert.AreEqual(0.5f, settings.Protection.HitChanceMultiplier);
            Assert.AreEqual(1.2f, settings.CaptureRadiusMetres);
            Assert.AreEqual(0.8f, settings.Placement.WallOffsetMetres);
            Assert.AreEqual(1, settings.Placement.PointsPerEdge);
        }

        [TestCase(1f)]
        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        public void Constructor_InvalidHitChance_Throws(float hitChance)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CoverSettings(CoverPlacement.Default, hitChance, CoverSettings.DefaultCaptureRadiusMetres));
        }

        [TestCase(0f)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_InvalidCaptureRadius_Throws(float radius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CoverSettings(CoverPlacement.Default, CoverSettings.DefaultHitChanceMultiplier, radius));
        }

        [Test]
        public void Constructor_NullPlacement_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CoverSettings(null, CoverSettings.DefaultHitChanceMultiplier, CoverSettings.DefaultCaptureRadiusMetres));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        public void Placement_InvalidWallOffset_Throws(float offset)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CoverPlacement(offset, CoverPlacement.DefaultPointsPerEdge));
        }

        [Test]
        public void Placement_ZeroPointsPerEdge_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CoverPlacement(CoverPlacement.DefaultWallOffsetMetres, 0));
        }

        [Test]
        public void CoverPoint_NonUnitNormal_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CoverPoint(Vector3.Zero, new Vector3(2f, 0f, 0f), CombatProtection.None));
        }
    }
}
