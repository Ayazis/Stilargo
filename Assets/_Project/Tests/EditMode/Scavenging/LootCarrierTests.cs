using System;
using NUnit.Framework;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Scavenging
{
    public sealed class LootCarrierTests
    {
        private const int CacheValue = 25;

        [Test]
        public void PickUp_TakesCacheValueAndLootsCache()
        {
            LootCache cache = new LootCache(new WorldPoint(0f, 0f), CacheValue);
            LootCarrier carrier = new LootCarrier();
            bool looted = false;
            cache.Looted += () => looted = true;

            carrier.PickUp(cache);

            Assert.AreEqual(CacheValue, carrier.CarriedValue);
            Assert.IsTrue(carrier.IsCarrying);
            Assert.IsTrue(cache.IsLooted);
            Assert.IsTrue(looted);
        }

        [Test]
        public void PickUp_WhileCarrying_Throws()
        {
            LootCarrier carrier = new LootCarrier();
            carrier.PickUp(new LootCache(new WorldPoint(0f, 0f), CacheValue));

            Assert.Throws<InvalidOperationException>(() => carrier.PickUp(new LootCache(new WorldPoint(1f, 1f), CacheValue)));
        }

        [Test]
        public void PickUp_LootedCache_Throws()
        {
            LootCache cache = new LootCache(new WorldPoint(0f, 0f), CacheValue);
            new LootCarrier().PickUp(cache);

            Assert.Throws<InvalidOperationException>(() => new LootCarrier().PickUp(cache));
        }

        [Test]
        public void DeliverTo_MovesValueIntoWalletAndEmptiesCarrier()
        {
            LootCarrier carrier = new LootCarrier();
            Wallet wallet = new Wallet();
            carrier.PickUp(new LootCache(new WorldPoint(0f, 0f), CacheValue));

            int delivered = carrier.DeliverTo(wallet);

            Assert.AreEqual(CacheValue, delivered);
            Assert.AreEqual(CacheValue, wallet.Balance);
            Assert.IsFalse(carrier.IsCarrying);
        }

        [Test]
        public void DeliverTo_WhenEmpty_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new LootCarrier().DeliverTo(new Wallet()));
        }

        [Test]
        public void LootCache_NonPositiveValue_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LootCache(new WorldPoint(0f, 0f), 0));
        }
    }
}
