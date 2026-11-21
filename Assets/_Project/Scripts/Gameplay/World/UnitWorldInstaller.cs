using System;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Economy;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Connects units to the world: the player's units see through the fog and scavenge loot,
    /// enemies hide in the fog, and the money label follows the wallet.
    /// List it after <see cref="WorldInstaller"/>, whose services exist only once it has installed.
    /// </summary>
    public sealed class UnitWorldInstaller : Installer
    {
        [SerializeField] private WorldInstaller _world;
        [SerializeField] private VisionSourceComponent[] _visionSources;
        [SerializeField] private ScavengerComponent[] _scavengers;
        [SerializeField] private FogHideable[] _fogHideables;
        [SerializeField] private WalletLabelView _walletLabelView;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            foreach (VisionSourceComponent visionSource in _visionSources)
            {
                visionSource.Construct(_world.VisibilityGrid);
            }

            foreach (ScavengerComponent scavenger in _scavengers)
            {
                scavenger.Construct(_world.ScavengingArea);
            }

            foreach (FogHideable hideable in _fogHideables)
            {
                hideable.Construct(_world.VisibilityGrid);
            }

            _walletLabelView.Construct(_world.Wallet);
        }

        private void ValidateReferences()
        {
            if (_world == null || _walletLabelView == null)
            {
                throw new InvalidOperationException($"{nameof(UnitWorldInstaller)} on '{name}' needs a world installer and a wallet label.");
            }

            if (_world.VisibilityGrid == null)
            {
                throw new InvalidOperationException($"{nameof(WorldInstaller)} has not installed yet; list it before {nameof(UnitWorldInstaller)}.");
            }

            RequireNoEmptySlots(_visionSources, "vision source");
            RequireNoEmptySlots(_scavengers, "scavenger");
            RequireNoEmptySlots(_fogHideables, "fog hideable");
        }

        private void RequireNoEmptySlots(UnityEngine.Object[] slots, string slotName)
        {
            if (slots == null)
            {
                throw new InvalidOperationException($"{nameof(UnitWorldInstaller)} on '{name}' has no {slotName} list.");
            }

            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] == null)
                {
                    throw new InvalidOperationException($"{nameof(UnitWorldInstaller)} on '{name}' has an empty {slotName} slot at index {index}.");
                }
            }
        }
    }
}
