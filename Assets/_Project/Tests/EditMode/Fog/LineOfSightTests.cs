using NUnit.Framework;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.Fog
{
    public sealed class LineOfSightTests
    {
        private const int Size = 12;
        private const float CellSize = 1f;
        private const float Radius = 10f;
        private const int WallX = 4;
        private const float WallHeightMetres = 6f;

        private VisibilityGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new VisibilityGrid(Size, Size, CellSize);
            _grid.UseSightBlocker(new WallBlocker(WallX, WallHeightMetres));
        }

        [Test]
        public void GroundSource_DoesNotSeePastATallerWall()
        {
            _grid.Register(new FakeVisionSource(1.5f, 5.5f, Radius));

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(3, 5));
            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(6, 5));
        }

        [Test]
        public void GroundSource_SeesTheWallItself()
        {
            _grid.Register(new FakeVisionSource(1.5f, 5.5f, Radius));

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(WallX, 5));
        }

        [Test]
        public void SourceAboveTheWall_SeesPastIt()
        {
            _grid.Register(new FakeVisionSource(1.5f, 5.5f, Radius) { EyeHeightMetres = WallHeightMetres + 1f });

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(6, 5));
        }

        [Test]
        public void WithoutABlocker_SightIsAPlainCircle()
        {
            var open = new VisibilityGrid(Size, Size, CellSize);
            open.Register(new FakeVisionSource(1.5f, 5.5f, Radius));

            open.Refresh();

            Assert.AreEqual(VisibilityState.Visible, open.GetState(6, 5));
        }

        [Test]
        public void UseSightBlocker_RejectsNull()
        {
            Assert.That(() => _grid.UseSightBlocker(null), Throws.ArgumentNullException);
        }

        private sealed class WallBlocker : ISightBlocker
        {
            private readonly int _wallX;
            private readonly float _height;

            public WallBlocker(int wallX, float height)
            {
                _wallX = wallX;
                _height = height;
            }

            public float ObstacleHeightAt(int cellX, int cellY)
            {
                return cellX == _wallX ? _height : 0f;
            }
        }
    }
}
