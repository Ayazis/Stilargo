using System;

namespace Stilargo.Core.Economy
{
    /// <summary>The player's money. Never negative; spending more than the balance is refused, not clamped.</summary>
    public sealed class Wallet
    {
        public event Action<int> BalanceChanged;

        public int Balance { get; private set; }

        public void Deposit(int amount)
        {
            RequirePositive(amount);
            Balance += amount;
            BalanceChanged?.Invoke(Balance);
        }

        /// <summary>Replaces the balance outright, for the settings menu. Zero is allowed; negative is not.</summary>
        public void SetBalance(int balance)
        {
            if (balance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(balance), balance, "Balance cannot be negative.");
            }

            Balance = balance;
            BalanceChanged?.Invoke(Balance);
        }

        public bool CanAfford(int amount)
        {
            RequirePositive(amount);
            return amount <= Balance;
        }

        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            Balance -= amount;
            BalanceChanged?.Invoke(Balance);
            return true;
        }

        private static void RequirePositive(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            }
        }
    }
}
