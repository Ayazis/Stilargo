using System.Numerics;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Turns a pointer near the window edge into a pan direction, C&amp;C style.
    /// </summary>
    public sealed class EdgeScroll
    {
        private const float TowardsMin = -1f;
        private const float TowardsMax = 1f;
        private const float None = 0f;

        private readonly float _marginPixels;

        public EdgeScroll(float marginPixels)
        {
            _marginPixels = SettingGuard.NonNegative(marginPixels, nameof(marginPixels));
        }

        /// <returns>Each axis -1, 0 or 1. X is right, Y is up the screen (forward).</returns>
        public Vector2 DirectionFor(Vector2 pointer, ScreenSize screen)
        {
            if (!screen.Contains(pointer))
            {
                return Vector2.Zero;
            }

            return new Vector2(AxisFor(pointer.X, screen.Width), AxisFor(pointer.Y, screen.Height));
        }

        private float AxisFor(float position, float extent)
        {
            if (position < _marginPixels)
            {
                return TowardsMin;
            }

            if (position > extent - _marginPixels)
            {
                return TowardsMax;
            }

            return None;
        }
    }
}
