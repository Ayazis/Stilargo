using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class NeighbourhoodTests
    {
        private const int Seed = 1234;

        private static List<TownBlock> Blocks(params BlockRole[] roles)
        {
            return roles.Select((role, index) => new TownBlock(new GridRect(new GridPosition(index * 20, 0), 12, 12), role)).ToList();
        }

        [Test]
        public void Pick_TurnsEveryOutskirtsBlockIntoANeighbourhood()
        {
            List<TownBlock> blocks = Blocks(BlockRole.MarketSquare, BlockRole.HouseBlock, BlockRole.Park, BlockRole.Outskirts, BlockRole.Outskirts, BlockRole.Outskirts);

            List<TownBlock> picked = NeighbourhoodPicker.Pick(blocks, new NeighbourhoodSettings(), new Random(Seed));

            Assert.IsFalse(picked.Any(block => block.Role == BlockRole.Outskirts));
            Assert.AreEqual(NeighbourhoodSettings.DefaultPlazaCount, picked.Count(block => block.Role == BlockRole.Plaza));
            Assert.AreEqual(BlockRole.MarketSquare, picked[0].Role);
            Assert.AreEqual(BlockRole.HouseBlock, picked[1].Role);
            Assert.AreEqual(BlockRole.Park, picked[2].Role);
        }

        [Test]
        public void Pick_UsesOnlyNeighbourhoodsWithWeight()
        {
            var settings = new NeighbourhoodSettings { PlazaCount = 0, OldTownWeight = 0, VillasWeight = 1, WorkshopsWeight = 0 };

            List<TownBlock> picked = NeighbourhoodPicker.Pick(Blocks(BlockRole.Outskirts, BlockRole.Outskirts, BlockRole.Outskirts), settings, new Random(Seed));

            Assert.IsTrue(picked.All(block => block.Role == BlockRole.Villas));
        }

        [Test]
        public void NeighbourhoodSettings_RejectsAllZeroWeights()
        {
            var settings = new NeighbourhoodSettings { OldTownWeight = 0, VillasWeight = 0, WorkshopsWeight = 0 };

            Assert.That(() => settings.Validate(), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void MixedBlockLayout_WithFullShareLeavesOnlyOpenGround()
        {
            var layout = new MixedBlockLayout(new PlotSplitter(new IntRange(3, 6)), new IntRange(2, 2), new OpenGroundShare(PlotUse.Yard, 1f));
            var plots = new List<Plot>();

            layout.Lay(new GridRect(new GridPosition(0, 0), 12, 12), new Random(Seed), plots);

            Assert.IsNotEmpty(plots);
            Assert.IsTrue(plots.All(plot => plot.Use == PlotUse.Yard && !plot.HasBuilding));
        }

        [Test]
        public void MixedBlockLayout_WithNoShareBuildsHousesOfTheGivenHeight()
        {
            var layout = new MixedBlockLayout(new PlotSplitter(new IntRange(3, 6)), new IntRange(2, 2), OpenGroundShare.None);
            var plots = new List<Plot>();

            layout.Lay(new GridRect(new GridPosition(0, 0), 12, 12), new Random(Seed), plots);

            Assert.IsTrue(plots.All(plot => plot.Use == PlotUse.House && plot.Storeys == 2));
            Assert.AreEqual(12 * 12, plots.Sum(plot => plot.Area.Width * plot.Area.Height));
        }

        [Test]
        public void HouseBlockLayout_RaisesOnlyTheCornerHouses()
        {
            var flat = new IntRange(3, 3);
            var settings = new RowHouseSettings(RowHouseSettings.DefaultRowDepthCells, new IntRange(3, 5), flat).WithCornerExtraStoreys(2);
            var plots = new List<Plot>();

            new HouseBlockLayout(settings).Lay(new GridRect(new GridPosition(0, 0), 12, 12), new Random(Seed), plots);

            var corners = new[] { new GridPosition(0, 0), new GridPosition(11, 0), new GridPosition(0, 11), new GridPosition(11, 11) };
            var houses = plots.Where(plot => plot.Use == PlotUse.House).ToList();
            Assert.IsTrue(houses.Where(plot => corners.Any(plot.Area.Contains)).All(plot => plot.Storeys == 5));
            Assert.IsTrue(houses.Where(plot => !corners.Any(plot.Area.Contains)).All(plot => plot.Storeys == 3));
        }
    }
}
