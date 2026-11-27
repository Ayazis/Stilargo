using System;

namespace Stilargo.Core.City
{
    /// <summary>An inclusive range of whole numbers. Max is never below Min.</summary>
    public readonly struct IntRange
    {
        public IntRange(int min, int max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, $"Must be at least the min ({min}).");
            }

            Min = min;
            Max = max;
        }

        public int Min { get; }

        public int Max { get; }

        public int Pick(Random random)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            return random.Next(Min, Max + 1);
        }

        public override string ToString()
        {
            return $"{Min}..{Max}";
        }
    }
}
