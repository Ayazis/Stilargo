using System;
using NUnit.Framework;
using Stilargo.Core.Fog;

namespace Stilargo.Tests.EditMode.Fog
{
    public sealed class VisibilityGridTests
    {
        private const int Size = 10;
        private const float CellSize = 1f;
        private const float Radius = 1.5f;

        private VisibilityGrid _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = new VisibilityGrid(Size, Size, CellSize);
        }

        [Test]
        public void NewGrid_AllCellsUnseen()
        {
            _grid.Refresh();

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(x, y));
                }
            }
        }

        [Test]
        public void Refresh_SourceRevealsCellsInRadiusOnly()
        {
            _grid.Register(new FakeVisionSource(2.5f, 2.5f, Radius));

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(2, 2));
            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(3, 2));
            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(3, 3));
            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(4, 4));
            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(9, 9));
            Assert.IsTrue(_grid.IsVisible(new WorldPoint(2.1f, 2.9f)));
            Assert.IsFalse(_grid.IsVisible(new WorldPoint(8f, 8f)));
        }

        [Test]
        public void Refresh_AfterSourceMovesAway_CellsBecomeExploredNotUnseen()
        {
            var source = new FakeVisionSource(2.5f, 2.5f, Radius);
            _grid.Register(source);
            _grid.Refresh();

            source.Position = new WorldPoint(8.5f, 8.5f);
            _grid.Refresh();
            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Explored, _grid.GetState(2, 2));
            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(8, 8));
            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(5, 5));
        }

        [Test]
        public void Unregister_SourceNoLongerReveals()
        {
            var source = new FakeVisionSource(2.5f, 2.5f, Radius);
            _grid.Register(source);
            _grid.Refresh();

            Assert.IsTrue(_grid.Unregister(source));
            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Explored, _grid.GetState(2, 2));
            Assert.IsFalse(_grid.Unregister(source));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        public void Refresh_NonPositiveRadius_RevealsNothing(float radius)
        {
            _grid.Register(new FakeVisionSource(2.5f, 2.5f, radius));

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(2, 2));
        }

        [Test]
        public void Refresh_TwoSources_BothAreasVisible()
        {
            _grid.Register(new FakeVisionSource(1.5f, 1.5f, Radius));
            _grid.Register(new FakeVisionSource(8.5f, 8.5f, Radius));

            _grid.Refresh();

            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(1, 1));
            Assert.AreEqual(VisibilityState.Visible, _grid.GetState(8, 8));
            Assert.AreEqual(VisibilityState.Unseen, _grid.GetState(5, 5));
        }

        [Test]
        public void Refresh_RaisesRefreshedEvent()
        {
            int raisedCount = 0;
            _grid.Refreshed += () => raisedCount++;

            _grid.Refresh();

            Assert.AreEqual(1, raisedCount);
        }

        [Test]
        public void Register_DuplicateOrNull_Throws()
        {
            var source = new FakeVisionSource(0f, 0f, Radius);
            _grid.Register(source);

            Assert.Throws<InvalidOperationException>(() => _grid.Register(source));
            Assert.Throws<ArgumentNullException>(() => _grid.Register(null));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(Size, 0)]
        [TestCase(0, Size)]
        public void GetState_OutOfRange_Throws(int x, int y)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _grid.GetState(x, y));
        }

        [Test]
        public void IsVisible_OutsideGrid_ReturnsFalse()
        {
            Assert.IsFalse(_grid.IsVisible(new WorldPoint(-5f, 50f)));
        }

        [TestCase(0, Size, CellSize)]
        [TestCase(Size, 0, CellSize)]
        [TestCase(Size, Size, 0f)]
        [TestCase(Size, Size, -1f)]
        [TestCase(Size, Size, float.NaN)]
        [TestCase(Size, Size, float.PositiveInfinity)]
        public void Constructor_InvalidArguments_Throws(int width, int height, float cellSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new VisibilityGrid(width, height, cellSize));
        }
    }
}
