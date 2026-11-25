using System;
using Stilargo.Core.Orders;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// Runs Core's <see cref="UnitOrders"/> on a NavMeshAgent and reports arrivals. Shared by player units and
    /// AI-driven units, so both detect "arrived" the same way.
    /// </summary>
    public sealed class NavMeshOrderDriver
    {
        // Slack on top of stoppingDistance so float noise near the target still counts as arrived.
        private const float ArrivalTolerance = 0.1f;
        // Covers a pivot at the body's centre, about a metre above the NavMesh, plus some slack.
        private const float NavMeshSampleRadius = 2f;

        private readonly NavMeshAgent _agent;
        private int _lastOrderFrame = -1;

        /// <summary>Places the agent on the NavMesh, so call it after the NavMesh is baked.</summary>
        public NavMeshOrderDriver(NavMeshAgent agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            _agent = agent;
            PlaceOnNavMesh();
            Orders = new UnitOrders(new NavMeshNavigationAgent(agent));

            // The driver owns these orders for its whole life, so this subscription never needs removing.
            Orders.WaypointsChanged += () => _lastOrderFrame = Time.frameCount;
        }

        public UnitOrders Orders { get; }

        /// <summary>Call every frame.</summary>
        public void ReportArrival()
        {
            if (Orders.IsIdle || !HasArrived())
            {
                return;
            }

            Orders.NotifyWaypointReached();
        }

        private bool HasArrived()
        {
            // A destination set this frame can report remainingDistance 0 before its path exists.
            if (_lastOrderFrame == Time.frameCount || _agent.pathPending)
            {
                return false;
            }

            return _agent.remainingDistance <= _agent.stoppingDistance + ArrivalTolerance;
        }

        private void PlaceOnNavMesh()
        {
            Vector3 position = _agent.transform.position;
            if (!NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                throw new InvalidOperationException($"{_agent.name} at {position} is not on the NavMesh.");
            }

            _agent.enabled = true;
            _agent.Warp(hit.position);
        }
    }
}
