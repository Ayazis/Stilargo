using System.Linq;
using System.Numerics;
using Stilargo.Core.Combat;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Sends every idle unit to the loot cache nearest the rival's base, within its scavenge radius, or home when it
    /// carries loot. Heads for the hill when the run is over, nothing is left to fetch, or the player alone holds it.
    /// </summary>
    public sealed class ScavengeStrategy : IRivalStrategy
    {
        private float _elapsedSeconds;

        public string Name => "Scavenging";

        public void Enter(RivalWorld world)
        {
            _elapsedSeconds = 0f;

            // With nothing to fetch, the next tick heads straight back to the hill, so don't pull units off it.
            if (!HasWorkLeft(world))
            {
                return;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive)
                {
                    Direct(world, unit);
                }
            }
        }

        public IRivalStrategy Tick(RivalWorld world, RivalStrategySet strategies, float deltaSeconds)
        {
            _elapsedSeconds += deltaSeconds;
            if (_elapsedSeconds >= world.Settings.ScavengeSeconds || world.Surroundings.IsPlayerHoldingHill || !HasWorkLeft(world))
            {
                return strategies.ContestHill;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive && unit.Orders.IsIdle)
                {
                    Direct(world, unit);
                }
            }

            return this;
        }

        private static bool HasWorkLeft(RivalWorld world)
        {
            return NearestCacheInRange(world) != null || world.Squad.Units.Any(unit => unit.IsAlive && unit.Carrier.IsCarrying);
        }

        private static void Direct(RivalWorld world, IRivalUnit unit)
        {
            Vector3 destination = DestinationFor(world, unit);
            RivalSquad.SendUnlessNear(unit, destination, world.Settings.ArrivalToleranceMetres);
        }

        private static Vector3 DestinationFor(RivalWorld world, IRivalUnit unit)
        {
            LootCache cache = NearestCacheInRange(world);
            if (unit.Carrier.IsCarrying || cache == null)
            {
                return world.Squad.BasePosition;
            }

            return ToGround(cache.Position);
        }

        private static LootCache NearestCacheInRange(RivalWorld world)
        {
            Vector3 home = world.Squad.BasePosition;
            LootCache cache = world.Surroundings.Scavenging.FindNearestAvailable(new WorldPoint(home.X, home.Z));
            if (cache == null || GroundDistance.Between(home, ToGround(cache.Position)) > world.Settings.ScavengeRadiusMetres)
            {
                return null;
            }

            return cache;
        }

        private static Vector3 ToGround(WorldPoint point)
        {
            return new Vector3(point.X, 0f, point.Z);
        }
    }
}
