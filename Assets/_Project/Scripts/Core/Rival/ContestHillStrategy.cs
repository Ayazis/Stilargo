using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Spreads the squad over the hill and sends back anyone who leaves it, e.g. after chasing a target. Once the
    /// contest time is up and the player does not hold the hill, it raids a standing house or else scavenges.
    /// </summary>
    public sealed class ContestHillStrategy : IRivalStrategy
    {
        private const float FullTurnRadians = 2f * (float)Math.PI;

        private float _elapsedSeconds;

        public string Name => "Contesting the hill";

        public void Enter(RivalWorld world)
        {
            _elapsedSeconds = 0f;
            IReadOnlyList<IRivalUnit> units = world.Squad.Units;
            for (int index = 0; index < units.Count; index++)
            {
                if (units[index].IsAlive)
                {
                    SendToSpot(world, index);
                }
            }
        }

        public IRivalStrategy Tick(RivalWorld world, RivalStrategySet strategies, float deltaSeconds)
        {
            _elapsedSeconds += deltaSeconds;
            RivalSurroundings surroundings = world.Surroundings;
            if (_elapsedSeconds >= world.Settings.ContestSeconds && !surroundings.IsPlayerHoldingHill)
            {
                return surroundings.IsHouseStanding ? strategies.RaidHouse : strategies.Scavenge;
            }

            RecallStragglers(world);
            return this;
        }

        private static void RecallStragglers(RivalWorld world)
        {
            IReadOnlyList<IRivalUnit> units = world.Squad.Units;
            for (int index = 0; index < units.Count; index++)
            {
                IRivalUnit unit = units[index];
                if (unit.IsAlive && unit.Orders.IsIdle && !world.Surroundings.Hill.Zone.Contains(unit.Position))
                {
                    SendToSpot(world, index);
                }
            }
        }

        private static void SendToSpot(RivalWorld world, int index)
        {
            IRivalUnit unit = world.Squad.Units[index];
            RivalSquad.SendUnlessNear(unit, SpotFor(world, index), world.Settings.ArrivalToleranceMetres);
        }

        /// <summary>Evenly spaced on a circle around the hill's centre, so units don't all queue for one point.</summary>
        private static Vector3 SpotFor(RivalWorld world, int index)
        {
            float angle = FullTurnRadians * index / world.Squad.Units.Count;
            float spread = world.Settings.HillSpreadMetres;
            Vector3 center = world.Surroundings.Hill.Zone.Center;
            return new Vector3(center.X + (spread * (float)Math.Cos(angle)), center.Y, center.Z + (spread * (float)Math.Sin(angle)));
        }
    }
}
