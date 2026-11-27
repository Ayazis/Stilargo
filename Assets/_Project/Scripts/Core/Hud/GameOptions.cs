using System;
using System.Collections.Generic;
using Stilargo.Core.Fog;
using Stilargo.Core.Input;

namespace Stilargo.Core.Hud
{
    /// <summary>The play options the settings page changes: the camera preset and whether the fog of war is on.</summary>
    public sealed class GameOptions
    {
        private readonly CameraControlChoice _cameraControls;
        private readonly VisibilityGrid _fog;

        public GameOptions(CameraControlChoice cameraControls, VisibilityGrid fog)
        {
            _cameraControls = cameraControls ?? throw new ArgumentNullException(nameof(cameraControls));
            _fog = fog ?? throw new ArgumentNullException(nameof(fog));
        }

        public string CameraPresetLabel => $"Camera: {_cameraControls.Current.Name}";

        public IReadOnlyList<ControlHint> CameraHints => CameraControlHints.For(_cameraControls.Current);

        public string FogLabel => _fog.IsFogEnabled ? "Fog of war: on" : "Fog of war: off";

        public void NextCameraPreset()
        {
            _cameraControls.Next();
        }

        public void ToggleFog()
        {
            if (_fog.IsFogEnabled)
            {
                _fog.DisableFog();
                return;
            }

            _fog.EnableFog();
        }
    }
}
