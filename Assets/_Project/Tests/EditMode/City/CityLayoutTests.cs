using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class CityLayoutTests
    {
        private const int Seed = 1234;

        // With the default config (2 m cells, 2-cell roads, 12-cell blocks) the border road covers 0-4 m
        // and the first block 4-28 m.
        private const float BorderRoadMetres = 1f;
        private const float FirstBlockMetres = 16f;
        private const float CellEdgeMetres = 4f;

        [Test]
        public void IsRoadAt_PointOnBorderRoad_ReturnsTrue()
        {
            CityLayout layout = CreateLayout();

            Assert.IsTrue(layout.IsRoadAt(BorderRoadMetres, FirstBlockMetres));
        }

        [Test]
        public void IsRoadAt_PointInsideBlock_ReturnsFalse()
        {
            CityLayout layout = CreateLayout();

            Assert.IsFalse(layout.IsRoadAt(FirstBlockMetres, FirstBlockMetres));
        }

        [Test]
        public void IsRoadAt_PointOnCellEdge_BelongsToTheCellAbove()
        {
            CityLayout layout = CreateLayout();

            Assert.IsFalse(layout.IsRoadAt(CellEdgeMetres, FirstBlockMetres));
        }

        [Test]
        public void IsRoadAt_PointWestOfTheCity_ReturnsFalse()
        {
            CityLayout layout = CreateLayout();

            Assert.IsFalse(layout.IsRoadAt(-BorderRoadMetres, FirstBlockMetres));
        }

        [Test]
        public void IsRoadAt_PointEastOfTheCity_ReturnsFalse()
        {
            CityLayout layout = CreateLayout();
            float beyondEastEdge = (layout.Width * layout.CellSize) + BorderRoadMetres;

            Assert.IsFalse(layout.IsRoadAt(beyondEastEdge, FirstBlockMetres));
        }

        private static CityLayout CreateLayout()
        {
            return new CityGenerator(Seed, new CityConfig()).Generate();
        }
    }
}
