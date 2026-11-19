using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class FakeUnitOrders : IUnitOrders
    {
        // Never raised: the group order tests only check which orders were given.
        public event Action DestinationReached
        {
            add { }
            remove { }
        }

        public bool IsIdle => MoveTargets.Count == 0 && QueuedTargets.Count == 0;

        public List<Vector3> MoveTargets { get; } = new List<Vector3>();

        public List<Vector3> QueuedTargets { get; } = new List<Vector3>();

        public int StopCount { get; private set; }

        public void MoveTo(Vector3 destination)
        {
            MoveTargets.Add(destination);
        }

        public void QueueMoveTo(Vector3 destination)
        {
            QueuedTargets.Add(destination);
        }

        public void Stop()
        {
            StopCount++;
        }
    }
}
