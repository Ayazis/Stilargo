using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class CityGeneratorTests
    {
        private const int Seed = 1234;
        private const int OtherSeed = 98765;

        [Test]
        public void Generate_SameSeed_ProducesIdenticalLayout()
        {
            CityLayout first = new CityGenerator(Seed, new CityConfig()).Generate();
            CityLayout second = new CityGenerator(Seed, new CityConfig()).Generate();

            Assert.AreEqual(first.Width, second.Width);
            Assert.AreEqual(first.Height, second.Height);
            CollectionAssert.AreEqual(first.Blocks, second.Blocks);
            CollectionAssert.AreEqual(first.RoadCells().ToList(), second.RoadCells().ToList());
            CollectionAssert.AreEqual(Describe(first), Describe(second));
        }

        [Test]
        public void Generate_DifferentSeeds_ProduceDifferentPlots()
        {
            CityLayout first = new CityGenerator(Seed, new CityConfig()).Generate();
            CityLayout second = new CityGenerator(OtherSeed, new CityConfig()).Generate();

            CollectionAssert.AreNotEqual(Describe(first), Describe(second));
        }

        [Test]
        public void Generate_RoadCells_FormOneConnectedComponent()
        {
            CityLayout layout = new CityGenerator(Seed, new CityConfig()).Generate();
            List<GridPosition> roads = layout.RoadCells().ToList();

            Assert.IsNotEmpty(roads);
            Assert.AreEqual(roads.Count, CountReachableRoadCells(layout, roads[0]));
        }

        [Test]
        public void Generate_Plots_NeverCoverRoads()
        {
            CityLayout layout = new CityGenerator(Seed, new CityConfig()).Generate();

            foreach (Plot plot in layout.Plots)
            {
                foreach (GridPosition cell in plot.Area.Cells())
                {
                    Assert.IsTrue(layout.IsInside(cell), $"Plot {plot.Area} leaves the grid.");
                    Assert.IsFalse(layout.IsRoad(cell), $"Plot {plot.Area} covers road at {cell}.");
                }
            }
        }

        [Test]
        public void Generate_Plots_NeverOverlapEachOther()
        {
            CityLayout layout = new CityGenerator(Seed, new CityConfig()).Generate();
            HashSet<GridPosition> claimed = new HashSet<GridPosition>();

            foreach (Plot plot in layout.Plots)
            {
                foreach (GridPosition cell in plot.Area.Cells())
                {
                    Assert.IsTrue(claimed.Add(cell), $"Cell {cell} belongs to more than one plot.");
                }
            }
        }

        [Test]
        public void Generate_OutskirtsPlots_RespectSizeRange()
        {
            CityConfig config = new CityConfig();
            var tooDeepForHouseBlocks = new RowHouseSettings(config.BlockSize, new IntRange(3, 5), new IntRange(3, 4));
            CityLayout layout = new CityGenerator(Seed, config, new TownSettings(0, tooDeepForHouseBlocks)).Generate();

            foreach (Plot plot in layout.Plots.Where(plot => plot.Use == PlotUse.House))
            {
                Assert.That(plot.Area.Width, Is.InRange(config.MinPlotSize, config.MaxPlotSize));
                Assert.That(plot.Area.Height, Is.InRange(config.MinPlotSize, config.MaxPlotSize));
                Assert.That(plot.Storeys, Is.InRange(config.MinStoreys, config.MaxStoreys));
            }
        }

        [Test]
        public void IsRoad_OutsideGrid_ReturnsFalse()
        {
            CityLayout layout = new CityGenerator(Seed, new CityConfig()).Generate();

            Assert.IsFalse(layout.IsRoad(new GridPosition(-1, 0)));
            Assert.IsFalse(layout.IsRoad(new GridPosition(layout.Width, 0)));
        }

        [Test]
        public void Constructor_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CityGenerator(Seed, null));
        }

        [Test]
        public void Constructor_InvalidConfig_Throws()
        {
            CityConfig config = new CityConfig { RoadWidth = 0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => new CityGenerator(Seed, config));
        }

        private static List<string> Describe(CityLayout layout)
        {
            return layout.Plots.Select(plot => $"{plot.Area}:{plot.Storeys}").ToList();
        }

        private static int CountReachableRoadCells(CityLayout layout, GridPosition start)
        {
            HashSet<GridPosition> visited = new HashSet<GridPosition> { start };
            Queue<GridPosition> frontier = new Queue<GridPosition>();
            frontier.Enqueue(start);
            while (frontier.Count > 0)
            {
                GridPosition current = frontier.Dequeue();
                foreach (GridPosition neighbour in Neighbours(current))
                {
                    if (layout.IsRoad(neighbour) && visited.Add(neighbour))
                    {
                        frontier.Enqueue(neighbour);
                    }
                }
            }

            return visited.Count;
        }

        private static IEnumerable<GridPosition> Neighbours(GridPosition cell)
        {
            yield return new GridPosition(cell.X + 1, cell.Y);
            yield return new GridPosition(cell.X - 1, cell.Y);
            yield return new GridPosition(cell.X, cell.Y + 1);
            yield return new GridPosition(cell.X, cell.Y - 1);
        }
    }
}
