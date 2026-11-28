using System;
using NUnit.Framework;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.Fog
{
    public sealed class SubdividedSightBlockerTests
    {
        private const float WallHeightMetres = 6f;

        [Test]
        public void ObstacleHeightAt_FogCellsInsideACityCell_ShareItsHeight()
        {
            var blocker = new SubdividedSightBlocker(new SingleWall(1, 0), 4);

            Assert.AreEqual(WallHeightMetres, blocker.ObstacleHeightAt(4, 0));
            Assert.AreEqual(WallHeightMetres, blocker.ObstacleHeightAt(7, 3));
            Assert.AreEqual(0f, blocker.ObstacleHeightAt(3, 0));
            Assert.AreEqual(0f, blocker.ObstacleHeightAt(8, 0));
        }

        [Test]
        public void ObstacleHeightAt_NegativeFogCell_MapsOutsideTheCity()
        {
            var blocker = new SubdividedSightBlocker(new SingleWall(-1, 0), 4);

            Assert.AreEqual(WallHeightMetres, blocker.ObstacleHeightAt(-1, 0));
        }

        [Test]
        public void Constructor_SplitBelowOne_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SubdividedSightBlocker(new SingleWall(0, 0), 0));
        }

        private sealed class SingleWall : ISightBlocker
        {
            private readonly int _x;
            private readonly int _y;

            public SingleWall(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public float ObstacleHeightAt(int cellX, int cellY)
            {
                return cellX == _x && cellY == _y ? WallHeightMetres : 0f;
            }
        }
    }
}
