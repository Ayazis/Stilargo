namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Per-frame facts the camera needs besides input: the window size and the elapsed time.
    /// </summary>
    public readonly struct CameraFrame
    {
        public CameraFrame(ScreenSize screen, float deltaTime)
        {
            Screen = screen;
            DeltaTime = SettingGuard.NonNegative(deltaTime, nameof(deltaTime));
        }

        public ScreenSize Screen { get; }

        public float DeltaTime { get; }
    }
}
