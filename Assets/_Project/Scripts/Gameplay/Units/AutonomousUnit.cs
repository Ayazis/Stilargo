using System;
using Stilargo.Core.Orders;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// A unit that takes move orders from code instead of the player, e.g. the AI rival's units.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AutonomousUnit : MonoBehaviour
    {
        private NavMeshOrderDriver _driver;

        /// <summary>Null until <see cref="Construct"/>.</summary>
        public IUnitOrders Orders => _driver == null ? null : _driver.Orders;

        /// <summary>Call after the NavMesh is baked.</summary>
        public void Construct()
        {
            if (_driver != null)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            _driver = new NavMeshOrderDriver(GetComponent<NavMeshAgent>());
        }

        private void Update()
        {
            if (_driver == null)
            {
                return;
            }

            _driver.ReportArrival();
        }
    }
}
