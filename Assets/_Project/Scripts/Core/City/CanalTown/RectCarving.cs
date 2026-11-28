using System.Collections.Generic;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>Cuts rectangles into non-overlapping pieces, so every cell belongs to exactly one plot.</summary>
    internal static class RectCarving
    {
        /// <summary>The parts of <paramref name="outer"/> around <paramref name="hole"/>: south and north strips, then west and east.</summary>
        public static IEnumerable<GridRect> Around(GridRect outer, GridRect hole)
        {
            if (hole.Min.Y > outer.Min.Y)
            {
                yield return Rect(outer.Min.X, outer.Min.Y, outer.Width, hole.Min.Y - outer.Min.Y);
            }

            if (outer.YMax > hole.YMax)
            {
                yield return Rect(outer.Min.X, hole.YMax, outer.Width, outer.YMax - hole.YMax);
            }

            if (hole.Min.X > outer.Min.X)
            {
                yield return Rect(outer.Min.X, hole.Min.Y, hole.Min.X - outer.Min.X, hole.Height);
            }

            if (outer.XMax > hole.XMax)
            {
                yield return Rect(hole.XMax, hole.Min.Y, outer.XMax - hole.XMax, hole.Height);
            }
        }

        /// <summary>A rectangle of the given size centred in <paramref name="outer"/>.</summary>
        public static GridRect Centred(GridRect outer, int width, int height)
        {
            return Rect(outer.Min.X + ((outer.Width - width) / 2), outer.Min.Y + ((outer.Height - height) / 2), width, height);
        }

        public static GridRect Rect(int x, int y, int width, int height)
        {
            return new GridRect(new GridPosition(x, y), width, height);
        }
    }
}
