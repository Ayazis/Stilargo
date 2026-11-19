using System;
using Stilargo.Core.Economy;

namespace Stilargo.Core.Scavenging
{
    /// <summary>What one unit is hauling. A unit carries one cache at a time, so scavenging means round trips.</summary>
    public sealed class LootCarrier
    {
        public int CarriedValue { get; private set; }

        public bool IsCarrying => CarriedValue > 0;

        public void PickUp(LootCache cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            if (IsCarrying)
            {
                throw new InvalidOperationException("Carrier is already carrying loot.");
            }

            CarriedValue = cache.Take();
        }

        public int DeliverTo(Wallet wallet)
        {
            if (wallet == null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }

            if (!IsCarrying)
            {
                throw new InvalidOperationException("Carrier has nothing to deliver.");
            }

            int delivered = CarriedValue;
            CarriedValue = 0;
            wallet.Deposit(delivered);
            return delivered;
        }
    }
}
