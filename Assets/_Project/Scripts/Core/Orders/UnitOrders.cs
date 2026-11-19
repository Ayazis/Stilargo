using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Orders
{
    /// <summary>
    /// Move orders of one unit: keeps the waypoint queue and drives the navigation agent through it.
    /// Gameplay reports arrivals with <see cref="NotifyWaypointReached"/>.
    /// </summary>
    public sealed class UnitOrders : IUnitOrders
    {
        private readonly WaypointQueue _queue = new WaypointQueue();
        private readonly INavigationAgent _agent;

        public UnitOrders(INavigationAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }

        public event Action DestinationReached;

        /// <summary>Raised whenever <see cref="Waypoints"/> changes, so a path view can redraw.</summary>
        public event Action WaypointsChanged;

        public bool IsIdle => !_queue.HasCurrent;

        /// <summary>Remaining waypoints, current first. A live read-only view.</summary>
        public IReadOnlyList<Vector3> Waypoints => _queue.Waypoints;

        public void MoveTo(Vector3 destination)
        {
            _queue.Replace(destination);
            _agent.SetDestination(destination);
            WaypointsChanged?.Invoke();
        }

        public void QueueMoveTo(Vector3 destination)
        {
            bool wasIdle = IsIdle;
            _queue.Enqueue(destination);

            if (wasIdle)
            {
                _agent.SetDestination(destination);
            }

            WaypointsChanged?.Invoke();
        }

        public void Stop()
        {
            _queue.Clear();
            _agent.Halt();
            WaypointsChanged?.Invoke();
        }

        /// <summary>
        /// Called when the agent arrives at the current waypoint. Only valid while not idle:
        /// an arrival without a waypoint means Gameplay lost track of the order state.
        /// </summary>
        public void NotifyWaypointReached()
        {
            if (IsIdle)
            {
                throw new InvalidOperationException("A waypoint was reported reached while the unit has no orders.");
            }

            bool hasNext = _queue.Advance();

            if (hasNext)
            {
                _agent.SetDestination(_queue.Current);
            }

            WaypointsChanged?.Invoke();

            if (!hasNext)
            {
                DestinationReached?.Invoke();
            }
        }
    }
}
