using System;
using System.Numerics;

namespace Stilargo.Core.Orders
{
    /// <summary>
    /// Orders a single unit can receive. Positions are world space, Y up.
    /// </summary>
    public interface IUnitOrders
    {
        bool IsIdle { get; }

        /// <summary>Raised when the unit reaches the last destination in its queue.</summary>
        event Action DestinationReached;

        /// <summary>Clears the queue and moves to the destination.</summary>
        void MoveTo(Vector3 destination);

        /// <summary>Adds the destination to the end of the queue.</summary>
        void QueueMoveTo(Vector3 destination);

        void Stop();
    }
}
