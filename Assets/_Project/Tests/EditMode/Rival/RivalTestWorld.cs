using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Rival;
using Stilargo.Core.Scavenging;
using Stilargo.Tests.EditMode.Combat;
using Stilargo.Tests.EditMode.Fortification;

namespace Stilargo.Tests.EditMode.Rival
{
    /// <summary>
    /// A rival with two units at its base (0, 0), one loot cache at (10, 0) and the hill at (0, 40).
    /// Strategy times are short and distinct so tests can step through them.
    /// </summary>
    internal sealed class RivalTestWorld
    {
        public const float ScavengeSeconds = 5f;
        public const float ContestSeconds = 6f;
        public const float RaidSeconds = 7f;
        public const float ConfiscateSeconds = 8f;
        public const float HillRadius = 4f;
        public const int CacheValue = 30;

        public static readonly Vector3 Base = Vector3.Zero;
        public static readonly Vector3 CachePosition = new Vector3(10f, 0f, 0f);
        public static readonly Vector3 HillCenter = new Vector3(0f, 0f, 40f);

        public RivalTestWorld()
        {
            Units = new[] { new FakeRivalUnit(Base), new FakeRivalUnit(Base) };
            Cache = new LootCache(new WorldPoint(CachePosition.X, CachePosition.Z), CacheValue);
            RivalWallet = new Wallet();
            var scavenging = new ScavengingArea(new[] { Cache }, new DropOff(new WorldPoint(Base.X, Base.Z), RivalWallet), 1f);
            Hill = new HillControl(new HillZone(HillCenter, HillRadius), new HillSettings(HillCenter.X, HillCenter.Z, HillRadius));
            Ledger = FortificationTestHouse.CreateLedger();
            Claim = new HouseClaim(Team.Blue, Ledger);
            RivalClaim = new HouseClaim(Team.Red, Ledger);
            Settings = new RivalSettings()
                .WithStrategyTimes(ScavengeSeconds, ContestSeconds, RaidSeconds)
                .WithConfiscateTime(ConfiscateSeconds)
                .WithArrivalTolerance(0.5f);
            var houses = new RivalHouses(Ledger, Claim, RivalClaim);
            World = new RivalWorld(new RivalSquad(Units, Base), new RivalSurroundings(Hill, houses, scavenging), Settings);
        }

        public FakeRivalUnit[] Units { get; }

        public LootCache Cache { get; }

        public Wallet RivalWallet { get; }

        public HillControl Hill { get; }

        public HouseLedger Ledger { get; }

        /// <summary>The player's (Blue) houses.</summary>
        public HouseClaim Claim { get; }

        /// <summary>The rival's (Red) houses.</summary>
        public HouseClaim RivalClaim { get; }

        public RivalSettings Settings { get; }

        public RivalWorld World { get; }

        public RivalStrategySet Strategies { get; } = new RivalStrategySet();

        public ClaimedHouse ClaimHouse()
        {
            Claim.TryClaim(FortificationTestHouse.CreateBuilding());
            return Claim.Houses[Claim.Houses.Count - 1];
        }

        /// <summary>Claims a house for the player and destroys it, leaving an unoccupied ruin.</summary>
        public ClaimedHouse CreateRuin()
        {
            ClaimedHouse house = ClaimHouse();
            house.TakeDamage(house.MaxHealth);
            return house;
        }

        public void PutPlayerOnHill()
        {
            var player = new ICombatTarget[] { new FakeCombatTarget(HillCenter, 1f) };
            Hill.Tick(0f, player, Array.Empty<ICombatTarget>());
        }

        public void ArriveAll()
        {
            foreach (FakeRivalUnit unit in Units)
            {
                if (!unit.Orders.IsIdle)
                {
                    unit.FakeOrders.Arrive();
                }
            }
        }
    }
}
