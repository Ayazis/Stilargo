using System;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class RoofTopBoxTests
    {
        private const float Top = 11f;
        private const float Tolerance = 0.001f;

        private static RoofTopBox Box()
        {
            return new RoofTopBox(new WorldPoint(0f, 0f), new WorldPoint(10f, 6f), Top);
        }

        [Test]
        public void TryGetHeight_InsideFootprint_IsTheTop()
        {
            Assert.IsTrue(Box().TryGetHeight(new WorldPoint(9f, 1f), out float height));
            Assert.AreEqual(Top, height, Tolerance);
        }

        [Test]
        public void TryGetHeight_OutsideFootprint_ReturnsFalse()
        {
            Assert.IsFalse(Box().TryGetHeight(new WorldPoint(10.5f, 3f), out _));
        }

        [Test]
        public void HighestUnder_DeckOnBox_StandsOnTop()
        {
            var roofs = new RoofHeights();
            roofs.Add(Box());

            Assert.AreEqual(Top, roofs.HighestUnder(new WorldPoint(5f, 3f), 1f, 0f), Tolerance);
        }

        [Test]
        public void Constructor_InvertedCorners_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RoofTopBox(new WorldPoint(5f, 0f), new WorldPoint(0f, 6f), Top));
        }
    }
}
