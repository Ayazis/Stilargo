using System;

namespace Stilargo.Core.Hud
{
    /// <summary>The wording of the numbers on the HUD, kept apart from the widgets so it can be tested.</summary>
    public static class HudText
    {
        private const int SecondsPerMinute = 60;

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

        public static string HouseHealth(float currentHealth, float maxHealth)
        {
            return $"House: {currentHealth:0} / {maxHealth:0} HP";
        }
    }
}
