using System.Numerics;
using Stilargo.Core.Orders;
using Stilargo.Core.Rival;
using Stilargo.Core.Scavenging;

namespace Stilargo.Tests.EditMode.Rival
{
    public sealed class FakeRivalUnit : IRivalUnit
    {
        public FakeRivalUnit(Vector3 position)
        {
            Position = position;
            FakeOrders = new FakeRivalOrders(this);
        }

        public Vector3 Position { get; set; }

        public bool IsAlive { get; set; } = true;

        public FakeRivalOrders FakeOrders { get; }

        public IUnitOrders Orders => FakeOrders;

        public LootCarrier Carrier { get; } = new LootCarrier();
    }
}
