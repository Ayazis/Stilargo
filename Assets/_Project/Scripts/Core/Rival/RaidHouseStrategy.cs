namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Marches the squad to the claimed house's entrance, where its units shoot the house. Returns to the hill when
    /// the raid time is up, the house is gone or the player takes the hill.
    /// </summary>
    public sealed class RaidHouseStrategy : IRivalStrategy
    {
        private float _elapsedSeconds;

        public string Name => "Raiding your house";

        public void Enter(RivalWorld world)
        {
            _elapsedSeconds = 0f;
            if (!world.Surroundings.IsHouseStanding)
            {
                return;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive)
                {
                    SendToHouse(world, unit);
                }
            }
        }

        public IRivalStrategy Tick(RivalWorld world, RivalStrategySet strategies, float deltaSeconds)
        {
            _elapsedSeconds += deltaSeconds;
            RivalSurroundings surroundings = world.Surroundings;
            if (!surroundings.IsHouseStanding || surroundings.IsPlayerHoldingHill || _elapsedSeconds >= world.Settings.RaidSeconds)
            {
                return strategies.ContestHill;
            }

            foreach (IRivalUnit unit in world.Squad.Units)
            {
                if (unit.IsAlive && unit.Orders.IsIdle)
                {
                    SendToHouse(world, unit);
                }
            }

            return this;
        }

        private static void SendToHouse(RivalWorld world, IRivalUnit unit)
        {
            RivalSquad.SendUnlessNear(unit, world.Surroundings.Claim.House.Position, world.Settings.ArrivalToleranceMetres);
        }
    }
}
