using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Rival
{
    /// <summary>The rival's units and the base they start from and bring loot back to.</summary>
    public sealed class RivalSquad
    {
        public RivalSquad(IReadOnlyList<IRivalUnit> units, Vector3 basePosition)
        {
            if (units == null)
            {
                throw new ArgumentNullException(nameof(units));
            }

            if (units.Count == 0 || units.Any(unit => unit == null))
            {
                throw new ArgumentException("A squad needs at least one unit and no null entries.", nameof(units));
            }

            Units = units.ToList();
            BasePosition = basePosition;
        }

        /// <summary>All units, dead ones included, so each keeps its index for spreading out.</summary>
        public IReadOnlyList<IRivalUnit> Units { get; }

        public Vector3 BasePosition { get; }

        public bool HasAliveUnits => Units.Any(unit => unit.IsAlive);

        /// <summary>Orders the unit to the destination unless it is already within the tolerance of it.</summary>
        public static void SendUnlessNear(IRivalUnit unit, Vector3 destination, float toleranceMetres)
        {
            if (GroundDistance.Between(unit.Position, destination) <= toleranceMetres)
            {
                return;
            }

            unit.Orders.MoveTo(destination);
        }
    }
}
