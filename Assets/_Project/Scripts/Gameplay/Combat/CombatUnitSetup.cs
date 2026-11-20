using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Orders;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>Everything a <see cref="CombatUnit"/> needs from the composition root.</summary>
    public sealed class CombatUnitSetup
    {
        public CombatUnitSetup(UnitCombat combat, Health health, IReadOnlyList<ICombatTarget> enemies)
        {
            Combat = combat ?? throw new ArgumentNullException(nameof(combat));
            Health = health ?? throw new ArgumentNullException(nameof(health));
            Enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
        }

        public UnitCombat Combat { get; }

        public Health Health { get; }

        /// <summary>Live list the installer mutates as units die.</summary>
        public IReadOnlyList<ICombatTarget> Enemies { get; }

        /// <summary>Player move orders; null for units that only fight (enemies).</summary>
        public IUnitOrders Orders { get; private set; }

        public CombatUnitSetup WithOrders(IUnitOrders orders)
        {
            Orders = orders ?? throw new ArgumentNullException(nameof(orders));
            return this;
        }
    }
}
