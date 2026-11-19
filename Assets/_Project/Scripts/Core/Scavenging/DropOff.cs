using System;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Scavenging
{
    /// <summary>Where carried loot turns into money. Later this becomes the claimed house.</summary>
    public sealed class DropOff
    {
        public DropOff(WorldPoint position, Wallet wallet)
        {
            Position = position;
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        }

        public WorldPoint Position { get; }

        public Wallet Wallet { get; }
    }
}
