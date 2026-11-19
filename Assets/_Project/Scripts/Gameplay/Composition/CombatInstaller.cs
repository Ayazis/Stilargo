using System;
using Stilargo.Gameplay.CameraControl;
using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Wires Ci's side of the game: the RTS camera now, units and combat in later steps.
    /// </summary>
    public sealed class CombatInstaller : Installer
    {
        [SerializeField] private RtsCameraRig _cameraRig;
        [SerializeField] private CameraSettings _cameraSettings;

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (_cameraRig == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no {nameof(RtsCameraRig)} assigned.");
            }

            if (_cameraSettings == null)
            {
                throw new InvalidOperationException($"{nameof(CombatInstaller)} on '{name}' has no {nameof(CameraSettings)} assigned.");
            }

            _cameraRig.Construct(context.PlayerInput, _cameraSettings);
        }
    }
}
