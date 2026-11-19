using System.Numerics;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// An axis-aligned rectangle in screen pixels, origin bottom-left. Min is always the lower-left corner.
    /// </summary>
    public readonly struct ScreenRect
    {
        private ScreenRect(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        public Vector2 Min { get; }

        public Vector2 Max { get; }

        public float Width => Max.X - Min.X;

        public float Height => Max.Y - Min.Y;

        /// <summary>Builds the rectangle spanned by two opposite corners, whichever way the drag went.</summary>
        public static ScreenRect FromCorners(Vector2 a, Vector2 b)
        {
            return new ScreenRect(Vector2.Min(a, b), Vector2.Max(a, b));
        }

        /// <summary>True when the point lies inside or exactly on an edge.</summary>
        public bool Contains(Vector2 point)
        {
            return point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;
        }
    }
}
