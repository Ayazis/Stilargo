using System.Numerics;
using Stilargo.Core.Orders;
using Stilargo.Core.Scavenging;

namespace Stilargo.Core.Rival
{
    /// <summary>One unit the rival commands. Positions are world space, Y up.</summary>
    public interface IRivalUnit
    {
        /// <summary>Only read while <see cref="IsAlive"/>; a destroyed unit can no longer report it.</summary>
        Vector3 Position { get; }

        bool IsAlive { get; }

        IUnitOrders Orders { get; }

        LootCarrier Carrier { get; }
    }
}
