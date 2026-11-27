using System;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class TownLayoutTests
    {
        private const int Seed = 1234;
        private const int OddBlocks = 5;

        private static CityConfig FiveByFive()
        {
            return new CityConfig { BlocksX = OddBlocks, BlocksY = OddBlocks };
        }

        [Test]
        public void TownPlan_PutsTheMarketInTheMiddleAndHouseBlocksAroundIt()
        {
            BlockRole[] roles = TownPlan.Assign(OddBlocks, OddBlocks, 0, new Random(Seed));

            Assert.AreEqual(BlockRole.MarketSquare, roles[(2 * OddBlocks) + 2]);
            Assert.AreEqual(1, roles.Count(role => role == BlockRole.MarketSquare));
            Assert.AreEqual(8, roles.Count(role => role == BlockRole.HouseBlock));
            Assert.AreEqual(16, roles.Count(role => role == BlockRole.Outskirts));
        }

        [Test]
        public void TownPlan_TurnsOutskirtsIntoParksFirst()
        {
            BlockRole[] roles = TownPlan.Assign(OddBlocks, OddBlocks, 3, new Random(Seed));

            Assert.AreEqual(3, roles.Count(role => role == BlockRole.Park));
            Assert.AreEqual(8, roles.Count(role => role == BlockRole.HouseBlock));
        }

        [Test]
        public void Generate_MarketSquareIsTheWholeMiddleBlock()
        {
            CityLayout layout = new CityGenerator(Seed, FiveByFive()).Generate();

            Plot market = layout.Plots.Single(plot => plot.Use == PlotUse.Market);
            Assert.AreEqual(BlockAt(layout, 2, 2), market.Area);
            Assert.IsFalse(market.HasBuilding);
        }

        [Test]
        public void Generate_EachParkIsAWholeBlock()
        {
            CityLayout layout = new CityGenerator(Seed, FiveByFive()).Generate();

            var parks = layout.Plots.Where(plot => plot.Use == PlotUse.Park).ToList();
            Assert.AreEqual(TownSettings.DefaultParkBlockCount, parks.Count);
            Assert.IsTrue(parks.All(park => layout.Blocks.Contains(park.Area)));
        }

        [Test]
        public void Generate_HouseBlocksUseTheTownHouseHeights()
        {
            CityConfig config = FiveByFive();
            config.MaxStoreys = 2;
            CityLayout layout = new CityGenerator(Seed, config).Generate();
            GridRect northOfSquare = BlockAt(layout, 2, 3);

            var rowHouses = layout.Plots.Where(plot => plot.HasBuilding && northOfSquare.Overlaps(plot.Area)).ToList();
            Assert.IsNotEmpty(rowHouses);
            Assert.IsTrue(rowHouses.All(plot => plot.Storeys >= RowHouseSettings.DefaultMinStoreys));
        }

        [Test]
        public void HouseBlockLayout_RingsTheBlockAroundACourtyardGarden()
        {
            var block = new GridRect(new GridPosition(0, 0), 12, 12);
            var plots = new System.Collections.Generic.List<Plot>();

            new HouseBlockLayout(new RowHouseSettings()).Lay(block, new Random(Seed), plots);

            int depth = RowHouseSettings.DefaultRowDepthCells;
            var houses = plots.Where(plot => plot.Use == PlotUse.House).ToList();
            Plot garden = plots.Single(plot => plot.Use == PlotUse.Garden);
            int covered = houses.Sum(plot => plot.Area.Width * plot.Area.Height);
            var courtyard = new GridRect(new GridPosition(depth, depth), 12 - (2 * depth), 12 - (2 * depth));
            Assert.AreEqual((12 * 12) - (courtyard.Width * courtyard.Height), covered);
            Assert.AreEqual(courtyard, garden.Area);
            Assert.IsFalse(garden.HasBuilding);
            Assert.IsTrue(houses.All(plot => Math.Min(plot.Area.Width, plot.Area.Height) <= depth));
        }

        [Test]
        public void HouseBlockLayout_KeepsFrontagesInRange()
        {
            var block = new GridRect(new GridPosition(0, 0), 12, 12);
            var plots = new System.Collections.Generic.List<Plot>();

            new HouseBlockLayout(new RowHouseSettings()).Lay(block, new Random(Seed), plots);

            int depth = RowHouseSettings.DefaultRowDepthCells;
            var southAndNorthRows = plots.Where(plot => plot.Area.Min.Y == 0 || plot.Area.YMax == 12).ToList();
            Assert.IsNotEmpty(southAndNorthRows);
            foreach (Plot plot in southAndNorthRows)
            {
                Assert.AreEqual(depth, plot.Area.Height);
                Assert.That(plot.Area.Width, Is.InRange(RowHouseSettings.DefaultMinFrontageCells, RowHouseSettings.DefaultMaxFrontageCells));
            }
        }

        // The street shaper merges and splits outskirts blocks, so blocks are found by grid cell, not list index.
        private static GridRect BlockAt(CityLayout layout, int blockX, int blockY)
        {
            CityConfig config = FiveByFive();
            int period = config.BlockSize + config.RoadWidth;
            var cell = new GridPosition(config.RoadWidth + (blockX * period), config.RoadWidth + (blockY * period));
            return layout.Blocks.Single(block => block.Contains(cell));
        }

        [Test]
        public void RowHouseSettings_RejectsAFrontageRangeThatCannotSplitEveryStreet()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RowHouseSettings(4, new IntRange(3, 4), new IntRange(3, 4)));
        }
    }
}
