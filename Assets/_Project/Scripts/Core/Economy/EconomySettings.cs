using System;

namespace Stilargo.Core.Economy
{
    /// <summary>Tunable economy values, loaded from <c>Economy.json</c>. Validated on construction.</summary>
    public sealed class EconomySettings
    {
        public const int DefaultStartingMoney = 1000;

        public EconomySettings(int startingMoney = DefaultStartingMoney)
        {
            if (startingMoney < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startingMoney), startingMoney, "Starting money must be zero or more.");
            }

            StartingMoney = startingMoney;
        }

        /// <summary>Money in the player's wallet when the match begins.</summary>
        public int StartingMoney { get; }
    }
}
