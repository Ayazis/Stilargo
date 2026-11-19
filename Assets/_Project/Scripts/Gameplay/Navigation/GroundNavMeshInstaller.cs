using Stilargo.Gameplay.Composition;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.Navigation
{
    /// <summary>
    /// Bakes the NavMesh of Main's temporary walkable ground. It stands in until FL's WorldInstaller
    /// (city NavMesh) is wired into Main. Must run before CombatInstaller, which places units on it.
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class GroundNavMeshInstaller : Installer
    {
        private NavMeshSurface _surface;

        private void Awake()
        {
            CacheComponents();
        }

        public override void Install(GameContext context)
        {
            // The bootstrapper runs earlier than this component's Awake, so cache here too.
            CacheComponents();
            _surface.BuildNavMesh();

            // BuildNavMesh only registers the data once the surface is enabled, which happens after the
            // bootstrapper; units are placed right after this, so register it now. AddData is idempotent.
            _surface.AddData();
        }

        private void CacheComponents()
        {
            if (_surface == null)
            {
                _surface = GetComponent<NavMeshSurface>();
            }
        }
    }
}
