using System;
using System.Numerics;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Selection
{
    /// <summary>
    /// Orders that do nothing; selection tests only need a unit to have some.
    /// </summary>
    public sealed class FakeUnitOrders : IUnitOrders
    {
        public event Action DestinationReached;

        public bool IsIdle => true;

        public void MoveTo(Vector3 destination)
        {
        }

        public void QueueMoveTo(Vector3 destination)
        {
        }

        public void Stop()
        {
        }

        public void RaiseDestinationReached()
        {
            DestinationReached?.Invoke();
        }
    }
}
