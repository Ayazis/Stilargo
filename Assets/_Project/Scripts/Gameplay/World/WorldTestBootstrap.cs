using System;
using Stilargo.Gameplay.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Stand-in composition root for the WorldTest scene, so city and fog can be played before
    /// Ci's GameBootstrapper and Main scene exist. Delete it together with that scene once they do.
    /// </summary>
    public sealed class WorldTestBootstrap : MonoBehaviour
    {
        [SerializeField] private WorldInstaller _worldInstaller;
        [SerializeField] private VisionSourceComponent[] _visionSources = Array.Empty<VisionSourceComponent>();

        private void Start()
        {
            if (_worldInstaller == null)
            {
                throw new InvalidOperationException($"{nameof(WorldTestBootstrap)} needs a world installer.");
            }

            _worldInstaller.Install();
            foreach (VisionSourceComponent visionSource in _visionSources)
            {
                visionSource.Construct(_worldInstaller.VisibilityGrid);
            }
        }
    }
}
