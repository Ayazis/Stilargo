namespace Stilargo.Core.Camera
{
    /// <summary>
    /// How fast the camera pans and how close to the screen edge the pointer must be to scroll.
    /// </summary>
    public sealed class PanSettings
    {
        /// <param name="speedPerHeight">
        /// World units per second for each unit of camera height. Scaling by height keeps
        /// the on-screen pan speed the same at every zoom level.
        /// </param>
        /// <param name="edgeMarginPixels">Width of the edge-scroll band in pixels. Zero disables edge scroll.</param>
        public PanSettings(float speedPerHeight, float edgeMarginPixels)
        {
            SpeedPerHeight = SettingGuard.Positive(speedPerHeight, nameof(speedPerHeight));
            EdgeMarginPixels = SettingGuard.NonNegative(edgeMarginPixels, nameof(edgeMarginPixels));
        }

        public float SpeedPerHeight { get; }

        public float EdgeMarginPixels { get; }
    }
}
