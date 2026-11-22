using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Cover;

namespace Stilargo.Tests.EditMode.Cover
{
    public sealed class CoverPointGeneratorTests
    {
        private const float CellSize = 4f;
        private const float Tolerance = 1e-4f;

        // Road (0,0) | plot (1,0) | road (2,0): default 0.8 m offset puts points 0.8 m off each wall.
        private const float WestPointX = 3.2f;
        private const float EastPointX = 8.8f;
        private const float RowCentreZ = 2f;

        [Test]
        public void Generate_RoadPlotRoad_AddsOnePointPerSharedEdge()
        {
            IReadOnlyList<CoverPoint> points = Generate(CreateRoadPlotRoad(), CoverSettings.Default);

            Assert.AreEqual(2, points.Count);
        }

        [Test]
        public void Generate_RoadPlotRoad_PlacesPointsOffTheWallCentredOnTheEdge()
        {
            IReadOnlyList<CoverPoint> points = Generate(CreateRoadPlotRoad(), CoverSettings.Default);

            AssertNear(new Vector3(WestPointX, 0f, RowCentreZ), points[0].Position);
            AssertNear(new Vector3(EastPointX, 0f, RowCentreZ), points[1].Position);
        }

        [Test]
        public void Generate_RoadPlotRoad_FacesAwayFromTheWall()
        {
            IReadOnlyList<CoverPoint> points = Generate(CreateRoadPlotRoad(), CoverSettings.Default);

            AssertNear(new Vector3(-1f, 0f, 0f), points[0].Normal);
            AssertNear(new Vector3(1f, 0f, 0f), points[1].Normal);
        }

        [Test]
        public void Generate_RoadPlotRoad_UsesSettingsProtection()
        {
            IReadOnlyList<CoverPoint> points = Generate(CreateRoadPlotRoad(), CoverSettings.Default);

            Assert.AreEqual(CoverSettings.DefaultHitChanceMultiplier, points[0].Protection.HitChanceMultiplier, Tolerance);
            Assert.AreEqual(1f, points[0].Protection.DamageMultiplier, Tolerance);
        }

        [Test]
        public void Generate_TwoPointsPerEdge_SpreadsThemAlongTheEdge()
        {
            CoverSettings settings = new CoverSettings(new CoverPlacement(CoverPlacement.DefaultWallOffsetMetres, 2), CoverSettings.DefaultHitChanceMultiplier, CoverSettings.DefaultCaptureRadiusMetres);

            IReadOnlyList<CoverPoint> points = Generate(CreateRoadPlotRoad(), settings);

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(1f, points[0].Position.Z, Tolerance);
            Assert.AreEqual(3f, points[1].Position.Z, Tolerance);
        }

        [Test]
        public void Generate_RoadNextToRoad_AddsNoPoints()
        {
            CityGrid grid = new CityGrid(2, 1, CellSize);
            grid.MarkRoad(new GridPosition(0, 0));
            grid.MarkRoad(new GridPosition(1, 0));

            IReadOnlyList<CoverPoint> points = Generate(new CityLayout(grid, new List<GridRect>(), new List<Plot>()), CoverSettings.Default);

            Assert.AreEqual(0, points.Count);
        }

        [Test]
        public void Generate_RoadNextToEmptyNonPlotCell_AddsNoPoints()
        {
            CityGrid grid = new CityGrid(2, 1, CellSize);
            grid.MarkRoad(new GridPosition(0, 0));

            IReadOnlyList<CoverPoint> points = Generate(new CityLayout(grid, new List<GridRect>(), new List<Plot>()), CoverSettings.Default);

            Assert.AreEqual(0, points.Count);
        }

        [Test]
        public void Generate_GeneratedCity_IsDeterministic()
        {
            CityLayout layout = new CityGenerator(1234, new CityConfig()).Generate();

            IReadOnlyList<CoverPoint> first = Generate(layout, CoverSettings.Default);
            IReadOnlyList<CoverPoint> second = Generate(layout, CoverSettings.Default);

            Assert.Greater(first.Count, 0);
            Assert.AreEqual(first.Count, second.Count);
            Assert.AreEqual(first[first.Count - 1].Position, second[second.Count - 1].Position);
        }

        [Test]
        public void Generate_OffsetOfHalfACell_Throws()
        {
            CoverSettings settings = new CoverSettings(new CoverPlacement(CellSize / 2f, 1), CoverSettings.DefaultHitChanceMultiplier, CoverSettings.DefaultCaptureRadiusMetres);

            Assert.Throws<InvalidOperationException>(() => Generate(CreateRoadPlotRoad(), settings));
        }

        private static IReadOnlyList<CoverPoint> Generate(CityLayout layout, CoverSettings settings)
        {
            return new CoverPointGenerator(settings).Generate(layout);
        }

        private static CityLayout CreateRoadPlotRoad()
        {
            CityGrid grid = new CityGrid(3, 1, CellSize);
            grid.MarkRoad(new GridPosition(0, 0));
            grid.MarkRoad(new GridPosition(2, 0));
            GridRect block = new GridRect(new GridPosition(1, 0), 1, 1);
            return new CityLayout(grid, new List<GridRect> { block }, new List<Plot> { new Plot(block, 1) });
        }

        private static void AssertNear(Vector3 expected, Vector3 actual)
        {
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, $"Expected {expected} but was {actual}.");
        }
    }
}
