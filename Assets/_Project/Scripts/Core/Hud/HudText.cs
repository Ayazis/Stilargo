using System;
using System.Globalization;

namespace Stilargo.Core.Hud
{
    /// <summary>The wording of the numbers on the HUD, kept apart from the widgets so it can be tested.</summary>
    public static class HudText
    {
        public const string BaseName = "Base";
        public const string HouseName = "House";

        private const int SecondsPerMinute = 60;
        private const float PercentPerWhole = 100f;

        public static string Money(int balance)
        {
            return $"Money: {balance}";
        }

        /// <summary>Whole seconds as mm:ss, rounded up so the timer never shows 00:00 before the hold is won.</summary>
        public static string HillTimer(float secondsRemaining)
        {
            if (float.IsNaN(secondsRemaining) || secondsRemaining < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(secondsRemaining), secondsRemaining, "Seconds must be a non-negative number.");
            }

            int wholeSeconds = (int)Math.Ceiling(secondsRemaining);
            return $"Hold the hill: {wholeSeconds / SecondsPerMinute:00}:{wholeSeconds % SecondsPerMinute:00}";
        }

        public static string HouseHealth(string houseName, float currentHealth, float maxHealth)
        {
            return $"{houseName}: {currentHealth:0} / {maxHealth:0} HP";
        }

        public static string Income(int claimedHouses, float moneyPerSecond)
        {
            return string.Format(CultureInfo.InvariantCulture, "Houses: {0} | Income: {1:0.0}/s", claimedHouses, moneyPerSecond);
        }

        /// <summary>Condition as a whole percentage, rounded up so a barely worn house does not read as worse than it is.</summary>
        public static string HouseUpkeep(float condition, float moneyPerSecond)
        {
            int conditionPercent = (int)Math.Ceiling(condition * PercentPerWhole);
            return string.Format(CultureInfo.InvariantCulture, "Condition: {0}% | Earns {1:0.0}/s", conditionPercent, moneyPerSecond);
        }
    }
}
