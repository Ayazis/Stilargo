using System;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Stand-in composition root for the WorldTest scene, so FL's world features can be played before
    /// Ci's GameBootstrapper and Main scene exist. Delete it together with that scene once they do.
    /// </summary>
    public sealed class WorldTestBootstrap : MonoBehaviour
    {
        [SerializeField] private WorldInstaller _worldInstaller;
        [SerializeField] private VisionSourceComponent _scout;
        [SerializeField] private WorldTestHud _hud;

        private void Start()
        {
            if (_worldInstaller == null || _scout == null || _hud == null)
            {
                throw new InvalidOperationException($"{nameof(WorldTestBootstrap)} needs a world installer, scout and HUD.");
            }

            _worldInstaller.Install();
            _scout.Construct(_worldInstaller.VisibilityGrid);

            ScavengerComponent scavenger = _scout.GetComponent<ScavengerComponent>();
            scavenger.Construct(_worldInstaller.ScavengingArea);
            _scout.GetComponent<WorldTestAutoScavenger>().Construct(_worldInstaller.ScavengingArea);
            _hud.Construct(_worldInstaller.Wallet, scavenger.Carrier);
        }
    }
}
