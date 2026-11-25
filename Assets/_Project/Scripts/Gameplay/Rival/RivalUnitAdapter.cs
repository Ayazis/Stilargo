using System;
using Stilargo.Core.Orders;
using Stilargo.Core.Rival;
using Stilargo.Core.Scavenging;
using Stilargo.Gameplay.Combat;

namespace Stilargo.Gameplay.Rival
{
    /// <summary>Presents an enemy <see cref="CombatUnit"/> and its AI orders to Core's rival brain.</summary>
    public sealed class RivalUnitAdapter : IRivalUnit
    {
        private readonly CombatUnit _unit;

        public RivalUnitAdapter(CombatUnit unit, IUnitOrders orders)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            _unit = unit;
            Orders = orders ?? throw new ArgumentNullException(nameof(orders));
        }

        public System.Numerics.Vector3 Position => _unit.Position;

        // A destroyed unit compares equal to null in Unity, so check that before asking its health.
        public bool IsAlive => _unit != null && _unit.IsAlive;

        public IUnitOrders Orders { get; }

        public LootCarrier Carrier { get; } = new LootCarrier();
    }
}
