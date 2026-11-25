using System;
using Stilargo.Core.Hud;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.World;
using UnityEngine;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// Wires the HUD to the services the other installers made. List it last. Step 10 calls
    /// <see cref="UseHillStatus"/> before this installer runs to make the hill timer appear.
    /// </summary>
    public sealed class HudInstaller : Installer
    {
        [SerializeField] private WorldInstaller _world;
        [SerializeField] private CombatInstaller _combat;
        [SerializeField] private FortificationInstaller _fortification;
        [SerializeField] private HudView _view;

        private IHillStatus _hillStatus = new NoHillStatus();

        public void UseHillStatus(IHillStatus hillStatus)
        {
            _hillStatus = hillStatus ?? throw new ArgumentNullException(nameof(hillStatus));
        }

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ValidateReferences();
            var stanceMenu = new StanceMenu(_combat.SelectionController.Selection, _combat.SelectionStance);
            HudContent content = new HudContent(_world.Wallet, stanceMenu, _fortification.FortifyMenu).WithHillStatus(_hillStatus);
            var pauseMenu = new PauseMenu(_world.Wallet, new ApplicationQuitter());
            _view.Construct(content, context.PlayerInput, pauseMenu);
            _combat.SelectionController.UseUiPointerBlocker(_view);
        }

        private void ValidateReferences()
        {
            if (_world == null || _combat == null || _fortification == null || _view == null)
            {
                throw new InvalidOperationException($"{nameof(HudInstaller)} on '{name}' needs the world, combat and fortification installers and a HudView.");
            }

            if (_world.Wallet == null || _combat.SelectionStance == null || _fortification.FortifyMenu == null)
            {
                throw new InvalidOperationException(
                    $"List {nameof(WorldInstaller)}, {nameof(CombatInstaller)} and {nameof(FortificationInstaller)} before {nameof(HudInstaller)}.");
            }
        }
    }
}
