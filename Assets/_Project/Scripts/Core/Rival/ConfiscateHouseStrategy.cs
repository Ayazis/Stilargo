using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Marches the squad to the nearest fallen house and claims it for the rival as soon as one unit reaches its
    /// entrance. Returns to the hill once it is claimed, someone else took it first, the time is up or the player
    /// takes the hill.
    /// </summary>
    public sealed class ConfiscateHouseStrategy : IRivalStrategy
    {
        private float _elapsedSeconds;
        private ClaimedHouse _target;

        public string Name => "Confiscating a house";

        public void Enter(RivalWorld world)
        {
            _elapsedSeconds = 0f;
            if (!world.Surroundings.Houses.TryFindNearestToConfiscate(SquadPosition(world), out _target))
            {
                return;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive)
                {
                    SendToTarget(world, unit);
                }
            }
        }

        public IRivalStrategy Tick(RivalWorld world, RivalStrategySet strategies, float deltaSeconds)
        {
            _elapsedSeconds += deltaSeconds;
            RivalSurroundings surroundings = world.Surroundings;
            if (surroundings.IsPlayerHoldingHill || _elapsedSeconds >= world.Settings.ConfiscateSeconds
                || !surroundings.Houses.IsConfiscatable(_target))
            {
                return strategies.ContestHill;
            }

            if (IsAnyUnitAtEntrance(world))
            {
                surroundings.Houses.OwnHouses.TryClaim(_target.Building);
                return strategies.ContestHill;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive && unit.Orders.IsIdle)
                {
                    SendToTarget(world, unit);
                }
            }

            return this;
        }

        private bool IsAnyUnitAtEntrance(RivalWorld world)
        {
            return world.Squad.Units.Any(unit =>
                unit.IsAlive && GroundDistance.Between(unit.Position, _target.Position) <= world.Settings.ArrivalToleranceMetres);
        }

        private void SendToTarget(RivalWorld world, IRivalUnit unit)
        {
            RivalSquad.SendUnlessNear(unit, _target.Position, world.Settings.ArrivalToleranceMetres);
        }

        // The first living unit stands in for the squad; strategies only run while one is alive.
        private static System.Numerics.Vector3 SquadPosition(RivalWorld world)
        {
            IRivalUnit leader = world.Squad.Units.FirstOrDefault(unit => unit.IsAlive);
            return leader == null ? world.Squad.BasePosition : leader.Position;
        }
    }
}
