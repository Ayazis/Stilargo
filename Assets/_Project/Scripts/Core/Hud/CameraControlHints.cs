using System;
using System.Collections.Generic;
using Stilargo.Core.Input;

namespace Stilargo.Core.Hud
{
    /// <summary>The camera controls the settings page lists, worded for the chosen preset.</summary>
    public static class CameraControlHints
    {
        public const string Title = "Camera controls";

        /// <summary>The hints for the default preset.</summary>
        public static IReadOnlyList<ControlHint> All { get; } = For(CameraControlPreset.Classic);

        public static IReadOnlyList<ControlHint> For(CameraControlPreset preset)
        {
            if (preset == null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            var hints = new List<ControlHint> { new ControlHint("Move", "W A S D / arrow keys") };
            if (preset.UsesEdgeScroll)
            {
                hints.Add(new ControlHint("Move (edge scroll)", "Mouse to screen edge"));
            }

            hints.Add(new ControlHint("Rotate", "Q / E"));
            hints.Add(new ControlHint("Orbit", preset.OrbitButton == OrbitButton.Right ? "Hold right mouse + drag" : "Hold middle mouse + drag"));
            hints.Add(new ControlHint("Zoom", "Mouse wheel"));
            return hints;
        }
    }
}
