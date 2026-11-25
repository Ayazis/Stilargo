using System.Collections.Generic;

namespace Stilargo.Core.Hud
{
    /// <summary>
    /// The camera controls shown on the settings page. Keep in step with the bindings in
    /// <c>InputSystemPlayerInput</c>; the list is read-only because rebinding is not supported yet.
    /// </summary>
    public static class CameraControlHints
    {
        public const string Title = "Camera controls";

        public static IReadOnlyList<ControlHint> All { get; } = new[]
        {
            new ControlHint("Move", "W A S D / arrow keys"),
            new ControlHint("Move (edge scroll)", "Mouse to screen edge"),
            new ControlHint("Rotate", "Q / E"),
            new ControlHint("Orbit", "Hold middle mouse + drag"),
            new ControlHint("Zoom", "Mouse wheel")
        };
    }
}
