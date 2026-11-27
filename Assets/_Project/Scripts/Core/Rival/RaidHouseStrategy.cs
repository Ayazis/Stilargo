using Stilargo.Core.Fortification;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Marches the squad to the entrance of the player's oldest standing house, where its units shoot it. Once the
    /// house falls the squad confiscates it; it returns to the hill when the raid time is up, nothing is left to
    /// raid or the player takes the hill.
    /// </summary>
    public sealed class RaidHouseStrategy : IRivalStrategy
    {
        private float _elapsedSeconds;
        private ClaimedHouse _target;

        public string Name => "Raiding your house";

        public void Enter(RivalWorld world)
        {
            _elapsedSeconds = 0f;
            if (!world.Surroundings.Houses.TryFindRaidTarget(out _target))
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
            if (surroundings.IsPlayerHoldingHill || _elapsedSeconds >= world.Settings.RaidSeconds)
            {
                return strategies.ContestHill;
            }

            if (!surroundings.Houses.IsRaidable(_target))
            {
                return surroundings.Houses.IsConfiscatable(_target) ? strategies.ConfiscateHouse : strategies.ContestHill;
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

        private void SendToTarget(RivalWorld world, IRivalUnit unit)
        {
            RivalSquad.SendUnlessNear(unit, _target.Position, world.Settings.ArrivalToleranceMetres);
        }
    }
}
