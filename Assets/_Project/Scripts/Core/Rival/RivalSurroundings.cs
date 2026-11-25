using System;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Rival
{
    /// <summary>What the rival reads about the world: the hill, the player's house and the loot it can take.</summary>
    public sealed class RivalSurroundings
    {
        public RivalSurroundings(HillControl hill, HouseClaim claim, ScavengingArea scavenging)
        {
            Hill = hill ?? throw new ArgumentNullException(nameof(hill));
            Claim = claim ?? throw new ArgumentNullException(nameof(claim));
            Scavenging = scavenging ?? throw new ArgumentNullException(nameof(scavenging));
        }

        public HillControl Hill { get; }

        public HouseClaim Claim { get; }

        /// <summary>The shared loot caches, with the rival's own drop-off and wallet.</summary>
        public ScavengingArea Scavenging { get; }

        public bool IsPlayerHoldingHill => Hill.State == HillState.HeldByPlayer;

        public bool IsHouseStanding => Claim.IsClaimed && Claim.House.IsAlive;
    }
}
