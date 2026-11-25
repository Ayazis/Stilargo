using System;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// The rival's state machine: runs the current strategy and switches when it names another. Loot is picked up
    /// and delivered by proximity whatever the strategy, so a unit on its way somewhere still scavenges.
    /// </summary>
    public sealed class RivalBrain
    {
        private readonly RivalWorld _world;
        private readonly RivalStrategySet _strategies;

        public RivalBrain(RivalWorld world, RivalStrategySet strategies)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
        }

        public event Action<IRivalStrategy> StrategyChanged;

        /// <summary>Null until <see cref="Start"/>.</summary>
        public IRivalStrategy Current { get; private set; }

        /// <summary>True once every unit of the squad is dead; the brain then gives no more orders.</summary>
        public bool IsDefeated => !_world.Squad.HasAliveUnits;

        public void Start()
        {
            if (Current != null)
            {
                throw new InvalidOperationException("The rival brain has already started.");
            }

            SwitchTo(_strategies.Initial);
        }

        public void Tick(float deltaSeconds)
        {
            if (!(deltaSeconds >= 0f) || float.IsInfinity(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Delta time must be zero or more and finite.");
            }

            if (Current == null)
            {
                throw new InvalidOperationException("Start the rival brain before ticking it.");
            }

            if (IsDefeated)
            {
                return;
            }

            Scavenge();
            IRivalStrategy next = Current.Tick(_world, _strategies, deltaSeconds);
            if (next != Current)
            {
                SwitchTo(next);
            }
        }

        private void Scavenge()
        {
            foreach (IRivalUnit unit in _world.Squad.Units)
            {
                if (unit.IsAlive)
                {
                    _world.Surroundings.Scavenging.Visit(unit.Carrier, new WorldPoint(unit.Position.X, unit.Position.Z));
                }
            }
        }

        private void SwitchTo(IRivalStrategy strategy)
        {
            Current = strategy ?? throw new InvalidOperationException("A rival strategy returned no next strategy.");
            Current.Enter(_world);
            StrategyChanged?.Invoke(Current);
        }
    }
}
