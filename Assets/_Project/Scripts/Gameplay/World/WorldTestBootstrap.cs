using System;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Wires the WorldTest scout and HUD to the world, which <c>GameBootstrapper</c> has already installed in Awake.
    /// It stands in for the unit wiring Ci's installer will do in Main; delete it together with the WorldTest scene.
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

            if (_worldInstaller.VisibilityGrid == null)
            {
                throw new InvalidOperationException($"{nameof(WorldInstaller)} has not been installed; is it in the bootstrapper's list?");
            }

            _scout.Construct(_worldInstaller.VisibilityGrid);
            ScavengerComponent scavenger = _scout.GetComponent<ScavengerComponent>();
            scavenger.Construct(_worldInstaller.ScavengingArea);
            _scout.GetComponent<WorldTestAutoScavenger>().Construct(_worldInstaller.ScavengingArea);
            _hud.Construct(_worldInstaller.Wallet, scavenger.Carrier);
        }
    }
}
