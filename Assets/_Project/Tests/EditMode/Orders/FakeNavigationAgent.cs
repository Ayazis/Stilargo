using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Orders;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class FakeNavigationAgent : INavigationAgent
    {
        public List<Vector3> Destinations { get; } = new List<Vector3>();

        public int HaltCount { get; private set; }

        public void SetDestination(Vector3 destination)
        {
            Destinations.Add(destination);
        }

        public void Halt()
        {
            HaltCount++;
        }
    }
}
