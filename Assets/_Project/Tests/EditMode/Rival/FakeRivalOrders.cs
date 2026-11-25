using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Rival
{
    /// <summary>Records move orders; a unit stays busy after an order until the test calls <see cref="Arrive"/>.</summary>
    public sealed class FakeRivalOrders : IUnitOrders
    {
        private readonly FakeRivalUnit _unit;

        public FakeRivalOrders(FakeRivalUnit unit)
        {
            _unit = unit;
        }

        public event Action DestinationReached;

        public bool IsIdle { get; private set; } = true;

        public List<Vector3> MoveTargets { get; } = new List<Vector3>();

        public Vector3 LastTarget => MoveTargets[MoveTargets.Count - 1];

        public void MoveTo(Vector3 destination)
        {
            MoveTargets.Add(destination);
            IsIdle = false;
        }

        public void QueueMoveTo(Vector3 destination)
        {
            MoveTo(destination);
        }

        public void Stop()
        {
            IsIdle = true;
        }

        /// <summary>Teleports the unit to its last target and goes idle.</summary>
        public void Arrive()
        {
            _unit.Position = LastTarget;
            IsIdle = true;
            DestinationReached?.Invoke();
        }
    }
}
