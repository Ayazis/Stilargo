using System;

namespace Stilargo.Core.Camera
{
    /// <summary>How fast the camera turns and tilts, and how far it may tilt.</summary>
    public sealed class OrbitSettings
    {
        /// <param name="turnDegreesPerSecond">Yaw speed while a turn key is held.</param>
        /// <param name="dragDegreesPerPixel">Yaw and pitch change per pixel of orbit-drag mouse movement.</param>
        /// <param name="pitch">The allowed tilt range.</param>
        public OrbitSettings(float turnDegreesPerSecond, float dragDegreesPerPixel, PitchLimits pitch)
        {
            TurnDegreesPerSecond = SettingGuard.Positive(turnDegreesPerSecond, nameof(turnDegreesPerSecond));
            DragDegreesPerPixel = SettingGuard.Positive(dragDegreesPerPixel, nameof(dragDegreesPerPixel));
            Pitch = pitch ?? throw new ArgumentNullException(nameof(pitch));
        }

        public float TurnDegreesPerSecond { get; }

        public float DragDegreesPerPixel { get; }

        public PitchLimits Pitch { get; }
    }
}
