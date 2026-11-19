using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Stilargo.Core.City;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Scavenging
{
    public sealed class LootPlacerTests
    {
        private const int CitySeed = 1234;
        private const int LootSeed = 77;
        private const int OtherLootSeed = 5150;

        private CityLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _layout = new CityGenerator(CitySeed, new CityConfig()).Generate();
        }

        [Test]
        public void Place_SameSeed_ProducesIdenticalCaches()
        {
            CollectionAssert.AreEqual(Describe(Place(LootSeed)), Describe(Place(LootSeed)));
        }

        [Test]
        public void Place_DifferentSeeds_ProduceDifferentCaches()
        {
            CollectionAssert.AreNotEqual(Describe(Place(LootSeed)), Describe(Place(OtherLootSeed)));
        }

        [Test]
        public void Place_ProducesConfiguredCountWithValuesInRange()
        {
            LootConfig config = new LootConfig();
            IReadOnlyList<LootCache> caches = new LootPlacer(LootSeed, config).Place(_layout);

            Assert.AreEqual(config.CacheCount, caches.Count);
            foreach (LootCache cache in caches)
            {
                Assert.That(cache.Value, Is.InRange(config.MinValue, config.MaxValue));
            }
        }

        [Test]
        public void Place_EveryCacheSitsOnARoadCell()
        {
            foreach (LootCache cache in Place(LootSeed))
            {
                GridPosition cell = new GridPosition(
                    (int)Math.Floor(cache.Position.X / _layout.CellSize),
                    (int)Math.Floor(cache.Position.Z / _layout.CellSize));
                Assert.IsTrue(_layout.IsRoad(cell), $"Cache at {cell} is not on a road.");
            }
        }

        [Test]
        public void Place_NoTwoCachesShareASpot()
        {
            List<string> spots = Describe(Place(LootSeed)).Select(spot => spot.Split(':')[0]).ToList();

            CollectionAssert.AllItemsAreUnique(spots);
        }

        [Test]
        public void Place_MoreCachesRequestedThanDoorsteps_CapsAtDoorsteps()
        {
            LootConfig config = new LootConfig { CacheCount = 10000 };

            IReadOnlyList<LootCache> caches = new LootPlacer(LootSeed, config).Place(_layout);

            Assert.That(caches.Count, Is.InRange(1, _layout.Plots.Count));
        }

        [Test]
        public void Constructor_InvalidConfig_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LootPlacer(LootSeed, new LootConfig { MinValue = 0 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LootPlacer(LootSeed, new LootConfig { MinValue = 5, MaxValue = 4 }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LootPlacer(LootSeed, new LootConfig { CacheCount = -1 }));
            Assert.Throws<ArgumentNullException>(() => new LootPlacer(LootSeed, null));
        }

        private IReadOnlyList<LootCache> Place(int seed)
        {
            return new LootPlacer(seed, new LootConfig()).Place(_layout);
        }

        private static List<string> Describe(IEnumerable<LootCache> caches)
        {
            return caches.Select(cache => $"{cache.Position.X},{cache.Position.Z}:{cache.Value}").ToList();
        }
    }
}
