using System;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;
using Stilargo.Gameplay.Scavenging;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Temporary autopilot for the WorldTest scene: walks the scout to the nearest cache, then home, until the
    /// city is picked clean. It stands in for Ci's move orders; delete it with the WorldTest scene.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent), typeof(ScavengerComponent))]
    public sealed class WorldTestAutoScavenger : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private ScavengerComponent _scavenger;
        private ScavengingArea _area;

        public void Construct(ScavengingArea area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            CacheComponents();
            _area = area;

            // The agent stays disabled in the scene because the NavMesh only exists after the city is baked.
            _agent.enabled = true;
            _agent.Warp(transform.position);
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            if (_agent != null)
            {
                return;
            }

            _agent = GetComponent<NavMeshAgent>();
            _scavenger = GetComponent<ScavengerComponent>();
        }

        private void Update()
        {
            if (_area == null)
            {
                return;
            }

            if (!TryGetTarget(out WorldPoint target))
            {
                _agent.ResetPath();
                return;
            }

            _agent.SetDestination(new Vector3(target.X, transform.position.y, target.Z));
        }

        private bool TryGetTarget(out WorldPoint target)
        {
            if (_scavenger.Carrier.IsCarrying)
            {
                target = _area.DropOff.Position;
                return true;
            }

            LootCache cache = _area.FindNearestAvailable(new WorldPoint(transform.position.x, transform.position.z));
            target = cache == null ? _area.DropOff.Position : cache.Position;
            return cache != null;
        }
    }
}
