using System;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Scavenging
{
    public sealed class ScavengingAreaTests
    {
        private const float Reach = 2f;
        private const int NearValue = 10;
        private const int FarValue = 40;

        private Wallet _wallet;
        private LootCache _nearCache;
        private LootCache _farCache;
        private ScavengingArea _area;
        private LootCarrier _carrier;

        [SetUp]
        public void SetUp()
        {
            _wallet = new Wallet();
            _nearCache = new LootCache(new WorldPoint(10f, 0f), NearValue);
            _farCache = new LootCache(new WorldPoint(50f, 0f), FarValue);
            DropOff home = new DropOff(new WorldPoint(0f, 0f), _wallet);
            _area = new ScavengingArea(new[] { _nearCache, _farCache }, home, Reach);
            _carrier = new LootCarrier();
        }

        [Test]
        public void Visit_OutOfReach_DoesNothing()
        {
            Assert.AreEqual(ScavengeResult.Nothing, _area.Visit(_carrier, new WorldPoint(5f, 0f)));
            Assert.IsFalse(_carrier.IsCarrying);
        }

        [Test]
        public void Visit_WithinReachOfCache_PicksItUp()
        {
            Assert.AreEqual(ScavengeResult.PickedUp, _area.Visit(_carrier, new WorldPoint(11.5f, 0f)));
            Assert.AreEqual(NearValue, _carrier.CarriedValue);
            Assert.IsTrue(_nearCache.IsLooted);
        }

        [Test]
        public void Visit_LoadedNearAnotherCache_DoesNotPickUpAgain()
        {
            _area.Visit(_carrier, new WorldPoint(10f, 0f));

            Assert.AreEqual(ScavengeResult.Nothing, _area.Visit(_carrier, new WorldPoint(50f, 0f)));
            Assert.IsFalse(_farCache.IsLooted);
        }

        [Test]
        public void Visit_LoadedAtDropOff_DeliversIntoWallet()
        {
            _area.Visit(_carrier, new WorldPoint(10f, 0f));

            Assert.AreEqual(ScavengeResult.Delivered, _area.Visit(_carrier, new WorldPoint(0.5f, 0.5f)));
            Assert.AreEqual(NearValue, _wallet.Balance);
            Assert.IsFalse(_carrier.IsCarrying);
        }

        [Test]
        public void Visit_EmptyAtDropOff_DoesNothing()
        {
            Assert.AreEqual(ScavengeResult.Nothing, _area.Visit(_carrier, new WorldPoint(0f, 0f)));
            Assert.AreEqual(0, _wallet.Balance);
        }

        [Test]
        public void FindNearestAvailable_SkipsLootedCaches()
        {
            Assert.AreSame(_nearCache, _area.FindNearestAvailable(new WorldPoint(0f, 0f)));

            _nearCache.Take();

            Assert.AreSame(_farCache, _area.FindNearestAvailable(new WorldPoint(0f, 0f)));
        }

        [Test]
        public void FindNearestAvailable_AllLooted_ReturnsNull()
        {
            _nearCache.Take();
            _farCache.Take();

            Assert.IsNull(_area.FindNearestAvailable(new WorldPoint(0f, 0f)));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_InvalidReach_Throws(float reach)
        {
            DropOff home = new DropOff(new WorldPoint(0f, 0f), _wallet);

            Assert.Throws<ArgumentOutOfRangeException>(() => new ScavengingArea(Array.Empty<LootCache>(), home, reach));
        }
    }
}
