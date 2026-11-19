using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

namespace Stilargo.Core.Orders
{
    /// <summary>
    /// The ordered destinations a unit still has to visit. The current waypoint is first.
    /// </summary>
    public sealed class WaypointQueue
    {
        private readonly List<Vector3> _waypoints = new List<Vector3>();
        private readonly ReadOnlyCollection<Vector3> _readOnlyWaypoints;

        public WaypointQueue()
        {
            _readOnlyWaypoints = _waypoints.AsReadOnly();
        }

        public bool HasCurrent => _waypoints.Count > 0;

        public int Count => _waypoints.Count;

        /// <summary>All waypoints, current first. A live read-only view, not a copy.</summary>
        public IReadOnlyList<Vector3> Waypoints => _readOnlyWaypoints;

        public Vector3 Current
        {
            get
            {
                ThrowIfEmpty();
                return _waypoints[0];
            }
        }

        /// <summary>Drops every waypoint and makes <paramref name="destination"/> the only one.</summary>
        public void Replace(Vector3 destination)
        {
            _waypoints.Clear();
            _waypoints.Add(destination);
        }

        public void Enqueue(Vector3 destination)
        {
            _waypoints.Add(destination);
        }

        public void Clear()
        {
            _waypoints.Clear();
        }

        /// <summary>Drops the current waypoint.</summary>
        /// <returns>True when another waypoint remains.</returns>
        public bool Advance()
        {
            ThrowIfEmpty();
            _waypoints.RemoveAt(0);
            return HasCurrent;
        }

        private void ThrowIfEmpty()
        {
            if (!HasCurrent)
            {
                throw new InvalidOperationException("The waypoint queue is empty.");
            }
        }
    }
}
