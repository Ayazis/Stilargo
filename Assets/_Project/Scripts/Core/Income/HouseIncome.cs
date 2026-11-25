using System;
using System.Linq;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Income
{
    /// <summary>
    /// Pays every claimed house's earnings into the wallet and wears the houses down over time, so income keeps
    /// depending on upkeep.
    /// </summary>
    public sealed class HouseIncome
    {
        private readonly HouseClaim _claim;
        private readonly HouseIncomeSettings _settings;
        private readonly Wallet _wallet;
        private float _unpaidIncome;

        public HouseIncome(HouseClaim claim, HouseIncomeSettings settings, Wallet wallet)
        {
            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Rate = new HouseIncomeRate(settings);
        }

        public HouseIncomeRate Rate { get; }

        /// <summary>What all claimed houses earn together right now.</summary>
        public float TotalMoneyPerSecond => _claim.Houses.Sum(house => Rate.MoneyPerSecond(house));

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be zero or more and finite.");
            }

            // Pay before wearing, so a tick earns at the condition the house had during it.
            Pay(TotalMoneyPerSecond * deltaSeconds);
            Wear(deltaSeconds);
        }

        private void Wear(float deltaSeconds)
        {
            float wear = deltaSeconds / _settings.FullWearSeconds;
            foreach (ClaimedHouse house in _claim.Houses.Where(house => house.IsAlive))
            {
                house.Wear(wear);
            }
        }

        // The wallet takes whole amounts, so fractions carry over to the next tick.
        private void Pay(float amount)
        {
            _unpaidIncome += amount;
            int payable = (int)Math.Floor(_unpaidIncome);
            if (payable <= 0)
            {
                return;
            }

            _unpaidIncome -= payable;
            _wallet.Deposit(payable);
        }
    }
}
