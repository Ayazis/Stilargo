using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Scavenging
{
    /// <summary>A pile of salvage at a fixed spot. It can be taken exactly once.</summary>
    public sealed class LootCache
    {
        public LootCache(WorldPoint position, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be positive.");
            }

            Position = position;
            Value = value;
        }

        public event Action Looted;

        public WorldPoint Position { get; }

        public int Value { get; }

        public bool IsLooted { get; private set; }

        public int Take()
        {
            if (IsLooted)
            {
                throw new InvalidOperationException("Loot cache is already looted.");
            }

            IsLooted = true;
            Looted?.Invoke();
            return Value;
        }
    }
}
