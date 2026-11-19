using System.Numerics;
using Stilargo.Core.Orders;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// A unit the player can select and give orders to. Positions are world space, Y up.
    /// </summary>
    public interface ISelectable
    {
        Vector3 Position { get; }

        IUnitOrders Orders { get; }

        bool IsSelected { get; }

        void Select();

        void Deselect();
    }
}
