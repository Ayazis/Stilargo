using System;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Hill;
using UnityEngine;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// Hands step 10's hill to the HUD. Gameplay can't reference the UI assembly, so this lives here and runs
    /// between <see cref="HillRivalInstaller"/> and <see cref="HudInstaller"/>.
    /// </summary>
    public sealed class HillHudLink : Installer
    {
        [SerializeField] private HillRivalInstaller _hill;
        [SerializeField] private HudInstaller _hud;

        public override void Install(GameContext context)
        {
            if (_hill == null || _hud == null)
            {
                throw new InvalidOperationException($"{nameof(HillHudLink)} on '{name}' needs the hill installer and the HUD installer.");
            }

            if (_hill.Hill == null)
            {
                throw new InvalidOperationException($"List {nameof(HillRivalInstaller)} before {nameof(HillHudLink)}.");
            }

            _hud.UseHillStatus(new HillStatus(_hill.Hill, _hill.Referee));
        }
    }
}
