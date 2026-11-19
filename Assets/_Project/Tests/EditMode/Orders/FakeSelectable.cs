using System.Numerics;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Orders
{
    public sealed class FakeSelectable : ISelectable
    {
        private readonly FakeUnitOrders _orders = new FakeUnitOrders();

        public FakeSelectable(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; set; }

        public IUnitOrders Orders => _orders;

        public FakeUnitOrders FakeOrders => _orders;

        public bool IsSelected { get; private set; }

        public void Select()
        {
            IsSelected = true;
        }

        public void Deselect()
        {
            IsSelected = false;
        }
    }
}
