using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;

namespace Stilargo.Tests.EditMode.City
{
    public sealed class StreetFurniturePlannerTests
    {
        private const int Seed = 1234;

        private static CityLayout Generate()
        {
            var streets = new StreetSettings { AlleyChance = 1f };
            var config = new CityConfig { BlocksX = 5, BlocksY = 5 };
            return new CityGenerator(Seed, config, new TownSettings().WithStreets(streets)).Generate();
        }

        [Test]
        public void Plan_IsTheSameForTheSameSeed()
        {
            CityLayout layout = Generate();
            var planner = new StreetFurniturePlanner(new StreetLifeSettings());

            CollectionAssert.AreEqual(planner.Plan(layout, Seed), planner.Plan(layout, Seed));
        }

        [Test]
        public void Plan_PutsPropsOnRoadsAgainstAPlotAndNeverInAOneCellAlley()
        {
            CityLayout layout = Generate();

            var props = new StreetFurniturePlanner(new StreetLifeSettings()).Plan(layout, Seed);

            Assert.IsNotEmpty(props);
            foreach (StreetProp prop in props)
            {
                var beside = new GridPosition(prop.Cell.X + prop.Towards.X, prop.Cell.Y + prop.Towards.Y);
                var behind = new GridPosition(prop.Cell.X - prop.Towards.X, prop.Cell.Y - prop.Towards.Y);
                Assert.IsTrue(layout.IsRoad(prop.Cell));
                Assert.IsFalse(layout.IsRoad(beside));
                Assert.IsTrue(layout.IsRoad(behind), $"Prop at {prop.Cell} fills a one-cell alley.");
            }
        }

        [Test]
        public void Plan_HasLampsAndTrees()
        {
            var props = new StreetFurniturePlanner(new StreetLifeSettings()).Plan(Generate(), Seed);

            Assert.IsTrue(props.Any(prop => prop.Kind == StreetPropKind.Lamp));
            Assert.IsTrue(props.Any(prop => prop.Kind == StreetPropKind.Tree));
        }

        [Test]
        public void Plan_WithoutClutterHasOnlyLampsAndTrees()
        {
            var settings = new StreetLifeSettings { ClutterChance = 0f };

            var props = new StreetFurniturePlanner(settings).Plan(Generate(), Seed);

            Assert.IsTrue(props.All(prop => prop.Kind == StreetPropKind.Lamp || prop.Kind == StreetPropKind.Tree));
        }

        [Test]
        public void StreetLifeSettings_RejectsZeroLampSpacing()
        {
            Assert.That(() => new StreetFurniturePlanner(new StreetLifeSettings { LampSpacingCells = 0 }), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }
    }
}
