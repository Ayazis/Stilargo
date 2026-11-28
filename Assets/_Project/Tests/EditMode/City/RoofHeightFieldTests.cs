using System;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class RoofHeightFieldTests
    {
        private const float Tolerance = 0.001f;
        private const float BaseHeight = 0.5f;

        // 2 columns x 2 rows of 1 m cells from (0, 0): low on the west, high on the east, a hole in the north-west.
        private static RoofGrid Grid()
        {
            return new RoofGrid(0f, 0f, 1f, 2, new[] { 4f, 6f, float.NaN, 6f });
        }

        [Test]
        public void TryGetHeight_Unturned_ReadsTheCellUnderThePoint()
        {
            var field = new RoofHeightField(Grid(), new WorldPoint(10f, 20f), BaseHeight, Facing.North);

            Assert.IsTrue(field.TryGetHeight(new WorldPoint(10.5f, 20.5f), out float west));
            Assert.IsTrue(field.TryGetHeight(new WorldPoint(11.5f, 20.5f), out float east));
            Assert.AreEqual(4f + BaseHeight, west, Tolerance);
            Assert.AreEqual(6f + BaseHeight, east, Tolerance);
        }

        [Test]
        public void TryGetHeight_FacingEast_TurnsLocalNorthToWorldEast()
        {
            var field = new RoofHeightField(Grid(), new WorldPoint(0f, 0f), 0f, Facing.East);

            // Local (0.5, 0.5) lands at world (0.5, -0.5); local (1.5, 0.5) at world (0.5, -1.5).
            Assert.IsTrue(field.TryGetHeight(new WorldPoint(0.5f, -0.5f), out float low));
            Assert.IsTrue(field.TryGetHeight(new WorldPoint(0.5f, -1.5f), out float high));
            Assert.AreEqual(4f, low, Tolerance);
            Assert.AreEqual(6f, high, Tolerance);
        }

        [Test]
        public void TryGetHeight_OnUncoveredCellOrOutside_ReturnsFalse()
        {
            var field = new RoofHeightField(Grid(), new WorldPoint(0f, 0f), 0f, Facing.North);

            Assert.IsFalse(field.TryGetHeight(new WorldPoint(0.5f, 1.5f), out _));
            Assert.IsFalse(field.TryGetHeight(new WorldPoint(-0.5f, 0.5f), out _));
        }

        [Test]
        public void HighestUnder_DeckOverSlope_RestsOnHighestTile()
        {
            var roofs = new RoofHeights();
            roofs.Add(new RoofHeightField(Grid(), new WorldPoint(0f, 0f), 0f, Facing.North));

            Assert.AreEqual(6f, roofs.HighestUnder(new WorldPoint(1f, 0.5f), 0.4f, 0f), Tolerance);
        }

        [Test]
        public void Constructor_PartialRow_Throws()
        {
            Assert.Throws<ArgumentException>(() => new RoofGrid(0f, 0f, 1f, 2, new[] { 1f, 2f, 3f }));
        }
    }
}
