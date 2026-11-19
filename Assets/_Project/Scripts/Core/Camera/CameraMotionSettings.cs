using System;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// All tuning for the RTS camera motion. Immutable and validated on construction.
    /// </summary>
    public sealed class CameraMotionSettings
    {
        public CameraMotionSettings(PanSettings pan, ZoomSettings zoom, CameraBounds bounds)
        {
            Pan = pan ?? throw new ArgumentNullException(nameof(pan));
            Zoom = zoom ?? throw new ArgumentNullException(nameof(zoom));
            Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
        }

        public PanSettings Pan { get; }

        public ZoomSettings Zoom { get; }

        public CameraBounds Bounds { get; }
    }
}
