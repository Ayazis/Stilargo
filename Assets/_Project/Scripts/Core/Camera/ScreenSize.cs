using System.Numerics;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// The game window size in pixels.
    /// </summary>
    public readonly struct ScreenSize
    {
        public ScreenSize(float width, float height)
        {
            Width = SettingGuard.NonNegative(width, nameof(width));
            Height = SettingGuard.NonNegative(height, nameof(height));
        }

        public float Width { get; }

        public float Height { get; }

        /// <summary>
        /// False for a pointer outside the window, which is where it reports when the window
        /// is unfocused or the mouse has left it. Always false for a zero-sized (minimized) window.
        /// </summary>
        public bool Contains(Vector2 point)
        {
            if (Width <= 0f || Height <= 0f)
            {
                return false;
            }

            return point.X >= 0f && point.X <= Width && point.Y >= 0f && point.Y <= Height;
        }
    }
}
