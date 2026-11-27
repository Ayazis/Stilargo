using System;
using System.Collections.Generic;

namespace Stilargo.Core.Input
{
    /// <summary>
    /// One way of steering the camera: which button orbits it and whether moving the mouse to the screen edge pans.
    /// Keys (WASD, arrows, Q/E) and the wheel work the same in every preset.
    /// </summary>
    public sealed class CameraControlPreset
    {
        public CameraControlPreset(string name, OrbitButton orbitButton, bool usesEdgeScroll)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A preset needs a name.", nameof(name));
            }

            Name = name;
            OrbitButton = orbitButton;
            UsesEdgeScroll = usesEdgeScroll;
        }

        public static CameraControlPreset Classic { get; } = new CameraControlPreset("Classic (middle mouse)", OrbitButton.Middle, true);

        public static CameraControlPreset RightMouse { get; } = new CameraControlPreset("Right mouse orbit", OrbitButton.Right, true);

        public static CameraControlPreset ClassicNoEdgeScroll { get; } = new CameraControlPreset("Middle mouse, no edge scroll", OrbitButton.Middle, false);

        public static CameraControlPreset RightMouseNoEdgeScroll { get; } = new CameraControlPreset("Right mouse, no edge scroll", OrbitButton.Right, false);

        /// <summary>Every preset, in the order the settings menu cycles through them.</summary>
        public static IReadOnlyList<CameraControlPreset> All { get; } = new[] { Classic, RightMouse, ClassicNoEdgeScroll, RightMouseNoEdgeScroll };

        public string Name { get; }

        public OrbitButton OrbitButton { get; }

        public bool UsesEdgeScroll { get; }
    }
}
