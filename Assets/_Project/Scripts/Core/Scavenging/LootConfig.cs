using System;

namespace Stilargo.Core.Scavenging
{
    /// <summary>Tunable parameters for <see cref="LootPlacer"/>. <see cref="Validate"/> throws on invalid values.</summary>
    public sealed class LootConfig
    {
        private const int DefaultCacheCount = 12;
        private const int DefaultMinValue = 10;
        private const int DefaultMaxValue = 50;

        public int CacheCount { get; set; } = DefaultCacheCount;

        public int MinValue { get; set; } = DefaultMinValue;

        public int MaxValue { get; set; } = DefaultMaxValue;

        public void Validate()
        {
            if (CacheCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(CacheCount), CacheCount, "Must not be negative.");
            }

            if (MinValue <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(MinValue), MinValue, "Must be positive.");
            }

            if (MaxValue < MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(MaxValue), MaxValue, "Must be at least MinValue.");
            }
        }
    }
}
