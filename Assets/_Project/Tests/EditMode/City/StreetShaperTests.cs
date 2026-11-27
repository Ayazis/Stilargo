using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class StreetShaperTests
    {
        private const int FiveBlocks = 5;
        private const float HomeMetres = 58f;
        private const float HillXMetres = 86f;
        private const float HillZMetres = 72f;

        private static CityConfig FiveByFive()
        {
            return new CityConfig { BlocksX = FiveBlocks, BlocksY = FiveBlocks };
        }

        private static CityLayout Generate(int seed, TownSettings town)
        {
            return new CityGenerator(seed, FiveByFive(), town).Generate();
        }

        private static TownSettings Plain()
        {
            return new TownSettings().WithStreets(new StreetSettings { MergedBlockPairs = 0, AlleyChance = 0f });
        }

        [TestCase(1)]
        [TestCase(1234)]
        [TestCase(98765)]
        public void Shape_KeepsEveryRoadConnected(int seed)
        {
            Assert.IsTrue(CityRoads.AreConnected(Generate(seed, new TownSettings())));
        }

        [TestCase(1)]
        [TestCase(1234)]
        [TestCase(98765)]
        public void Shape_LeavesTheRoadsAroundTheCentreUnchanged(int seed)
        {
            CityLayout plain = Generate(seed, Plain());
            CityLayout shaped = Generate(seed, new TownSettings());
            CityConfig config = FiveByFive();
            int period = config.BlockSize + config.RoadWidth;
            int centreMin = period;
            int centreMax = (4 * period) + config.RoadWidth;

            for (int y = centreMin; y < centreMax; y++)
            {
                for (int x = centreMin; x < centreMax; x++)
                {
                    var cell = new GridPosition(x, y);
                    Assert.AreEqual(plain.IsRoad(cell), shaped.IsRoad(cell), $"Road changed at {cell}.");
                }
            }
        }

        [TestCase(1)]
        [TestCase(1234)]
        [TestCase(98765)]
        public void Shape_KeepsHomeAndTheHillOnARoad(int seed)
        {
            CityLayout layout = Generate(seed, new TownSettings());

            Assert.IsTrue(layout.IsRoad(CityRoads.CellAtMetres(layout, HomeMetres, HomeMetres)));
            Assert.IsTrue(layout.IsRoad(CityRoads.CellAtMetres(layout, HillXMetres, HillZMetres)));
        }

        [Test]
        public void Shape_MergesTheAskedNumberOfBlockPairs()
        {
            var streets = new StreetSettings { MergedBlockPairs = 2, AlleyChance = 0f };

            CityLayout layout = Generate(1234, new TownSettings().WithStreets(streets));

            Assert.AreEqual((FiveBlocks * FiveBlocks) - 2, layout.Blocks.Count);
        }

        [Test]
        public void Shape_WithAlwaysAlleysSplitsBlocksWithoutOverlap()
        {
            var streets = new StreetSettings { MergedBlockPairs = 0, AlleyChance = 1f };

            CityLayout layout = Generate(1234, new TownSettings().WithStreets(streets));

            Assert.Greater(layout.Blocks.Count, FiveBlocks * FiveBlocks);
            for (int first = 0; first < layout.Blocks.Count; first++)
            {
                for (int second = first + 1; second < layout.Blocks.Count; second++)
                {
                    Assert.IsFalse(layout.Blocks[first].Overlaps(layout.Blocks[second]));
                }
            }

            Assert.IsFalse(layout.Blocks.SelectMany(block => block.Cells()).Any(layout.IsRoad));
        }

        [Test]
        public void StreetSettings_RejectsAnAlleyChanceAboveOne()
        {
            Assert.That(() => new StreetSettings { AlleyChance = 1.5f }.Validate(), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }
    }
}
