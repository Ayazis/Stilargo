namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Which way the camera faces. Yaw 0 looks towards world +Z and grows clockwise seen from above;
    /// pitch is degrees down from the horizon, 90 being straight down.
    /// </summary>
    public readonly struct CameraAngles
    {
        public CameraAngles(float yawDegrees, float pitchDegrees)
        {
            YawDegrees = SettingGuard.Finite(yawDegrees, nameof(yawDegrees));
            PitchDegrees = SettingGuard.Finite(pitchDegrees, nameof(pitchDegrees));
        }

        public float YawDegrees { get; }

        public float PitchDegrees { get; }
    }
}
