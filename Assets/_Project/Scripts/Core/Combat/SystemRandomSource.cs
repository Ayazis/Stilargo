using System;

namespace Stilargo.Core.Combat
{
    /// <summary><see cref="IRandomSource"/> backed by <see cref="System.Random"/>.</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private const float LargestBelowOne = 0.99999994f;

        private readonly Random _random;

        public SystemRandomSource()
        {
            _random = new Random();
        }

        public SystemRandomSource(int seed)
        {
            _random = new Random(seed);
        }

        public float NextFloat()
        {
            // Casting a double just below 1 to float can round up to 1; clamp to keep the [0, 1) contract.
            float value = (float)_random.NextDouble();
            return Math.Min(value, LargestBelowOne);
        }
    }
}
