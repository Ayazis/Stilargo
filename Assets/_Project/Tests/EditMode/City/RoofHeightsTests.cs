using System;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class RoofHeightsTests
    {
        private const float EavesHeight = 9f;
        private const float Rise = 2f;
        private const float Tolerance = 0.001f;

        // 12 m along X, 8 m along Z: the ridge runs along X at Z = 0, from X = -2 to X = 2.
        private static HipRoofSurface Roof()
        {
            return new HipRoofSurface(new WorldPoint(0f, 0f), EavesHeight, 12f, 8f, Rise);
        }

        [Test]
        public void TryGetHeight_OnRidge_IsEavesPlusRise()
        {
            Assert.IsTrue(Roof().TryGetHeight(new WorldPoint(1f, 0f), out float height));
            Assert.AreEqual(EavesHeight + Rise, height, Tolerance);
        }

        [Test]
        public void TryGetHeight_HalfwayUpSlope_IsHalfTheRise()
        {
            Roof().TryGetHeight(new WorldPoint(0f, 2f), out float side);
            Roof().TryGetHeight(new WorldPoint(4f, 0f), out float hipEnd);

            Assert.AreEqual(EavesHeight + (Rise / 2f), side, Tolerance);
            Assert.AreEqual(EavesHeight + (Rise / 2f), hipEnd, Tolerance);
        }

        [Test]
        public void TryGetHeight_OutsideFootprint_ReturnsFalse()
        {
            Assert.IsFalse(Roof().TryGetHeight(new WorldPoint(0f, 4.5f), out _));
        }

        [Test]
        public void InvalidSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HipRoofSurface(new WorldPoint(0f, 0f), 0f, 0f, 8f, Rise));
        }

        [Test]
        public void HighestUnder_DeckOnSlope_ClearsItsUphillEdge()
        {
            var roofs = new RoofHeights();
            roofs.Add(Roof());

            float top = roofs.HighestUnder(new WorldPoint(0f, 2f), 0.5f, 0f);

            // The uphill edge is 2.5 m from the eave, 0.5 m per metre of rise.
            Assert.AreEqual(EavesHeight + 1.25f, top, Tolerance);
        }

        [Test]
        public void HighestUnder_NoRoof_ReturnsFallback()
        {
            Assert.AreEqual(3f, new RoofHeights().HighestUnder(new WorldPoint(50f, 50f), 0.5f, 3f));
        }
    }
}
