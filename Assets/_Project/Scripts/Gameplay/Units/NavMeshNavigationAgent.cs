using System;
using Stilargo.Core.Orders;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// Drives a NavMeshAgent for Core's <see cref="UnitOrders"/>.
    /// </summary>
    public sealed class NavMeshNavigationAgent : INavigationAgent
    {
        private readonly NavMeshAgent _agent;

        public NavMeshNavigationAgent(NavMeshAgent agent)
        {
            if (agent == null)
            {
                throw new ArgumentNullException(nameof(agent));
            }

            _agent = agent;
        }

        public void SetDestination(System.Numerics.Vector3 destination)
        {
            _agent.isStopped = false;

            if (!_agent.SetDestination(destination.ToUnity()))
            {
                throw new InvalidOperationException($"{_agent.name} could not request a path to {destination}.");
            }
        }

        public void Halt()
        {
            _agent.ResetPath();
        }
    }
}
