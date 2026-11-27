using System;
using Stilargo.Core.Hill;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Rival
{
    /// <summary>What the rival reads about the world: the hill, the houses and the loot it can take.</summary>
    public sealed class RivalSurroundings
    {
        public RivalSurroundings(HillControl hill, RivalHouses houses, ScavengingArea scavenging)
        {
            Hill = hill ?? throw new ArgumentNullException(nameof(hill));
            Houses = houses ?? throw new ArgumentNullException(nameof(houses));
            Scavenging = scavenging ?? throw new ArgumentNullException(nameof(scavenging));
        }

        public HillControl Hill { get; }

        public RivalHouses Houses { get; }

        /// <summary>The shared loot caches, with the rival's own drop-off and wallet.</summary>
        public ScavengingArea Scavenging { get; }

        public bool IsPlayerHoldingHill => Hill.State == HillState.HeldByPlayer;
    }
}
