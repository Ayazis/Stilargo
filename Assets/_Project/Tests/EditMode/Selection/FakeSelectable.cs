using System.Numerics;
using Stilargo.Core.Orders;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    /// <summary>
    /// A selectable that counts Select/Deselect calls so tests can check the selection drives the units.
    /// </summary>
    public sealed class FakeSelectable : ISelectable
    {
        public FakeSelectable()
            : this(Vector3.Zero)
        {
        }

        public FakeSelectable(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }

        public IUnitOrders Orders { get; } = new FakeUnitOrders();

        public bool IsSelected { get; private set; }

        public int SelectCount { get; private set; }

        public int DeselectCount { get; private set; }

        public void Select()
        {
            IsSelected = true;
            SelectCount++;
        }

        public void Deselect()
        {
            IsSelected = false;
            DeselectCount++;
        }
    }
}
