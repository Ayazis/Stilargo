using System;
using System.Collections.Generic;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.Orders;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Makes units leave their building, or stop heading to one, when given a new move order.</summary>
    public sealed class GarrisonExitOnMove : IDisposable
    {
        private readonly MoveOrderController _moveOrders;
        private readonly GarrisonOccupantRegistry _occupants;
        private bool _isDisposed;

        public GarrisonExitOnMove(MoveOrderController moveOrders, GarrisonOccupantRegistry occupants)
        {
            if (moveOrders == null)
            {
                throw new ArgumentNullException(nameof(moveOrders));
            }

            _moveOrders = moveOrders;
            _occupants = occupants ?? throw new ArgumentNullException(nameof(occupants));
            _moveOrders.UnitsMoveOrdered += HandleUnitsMoveOrdered;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (_moveOrders != null)
            {
                _moveOrders.UnitsMoveOrdered -= HandleUnitsMoveOrdered;
            }
        }

        private void HandleUnitsMoveOrdered(IReadOnlyList<ISelectable> units)
        {
            foreach (ISelectable unit in units)
            {
                if (_occupants.TryGet(unit, out GarrisonOccupant occupant))
                {
                    occupant.Leave();
                }
            }
        }
    }
}
