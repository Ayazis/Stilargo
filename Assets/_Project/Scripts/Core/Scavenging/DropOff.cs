using System;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Scavenging
{
    /// <summary>Where carried loot turns into money. Moves to the claimed house once the player claims one.</summary>
    public sealed class DropOff
    {
        public DropOff(WorldPoint position, Wallet wallet)
        {
            Position = position;
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public WorldPoint Position { get; private set; }

        public Wallet Wallet { get; }

        public void Relocate(WorldPoint position)
        {
            Position = position;
        }
    }
}
