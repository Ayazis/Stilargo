using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Where on a plot to put props: spots on a jittered grid in a shuffled order, and random headings.</summary>
    internal static class PropScatter
    {
        private const float Half = 0.5f;
        private const float JitterShare = 0.25f;
        private const float FullTurnDegrees = 360f;
        private const float QuarterTurnDegrees = 90f;

        /// <summary>
        /// Ground points about <paramref name="spacing"/> apart inside the plot, kept <paramref name="margin"/> from its
        /// edges, in random order; take as many as needed from the front.
        /// </summary>
        public static List<Vector3> Spots(PlotFrame frame, float spacing, float margin, System.Random random)
        {
            float width = frame.Width - (margin * 2f);
            float depth = frame.Depth - (margin * 2f);
            if (width <= 0f || depth <= 0f)
            {
                return new List<Vector3>();
            }

            int columns = Math.Max(1, (int)(width / spacing));
            int rows = Math.Max(1, (int)(depth / spacing));
            var spots = new List<Vector3>();
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    float x = frame.West + margin + (width * (column + Half) / columns) + Jitter(random, width / columns);
                    float z = frame.South + margin + (depth * (row + Half) / rows) + Jitter(random, depth / rows);
                    spots.Add(new Vector3(x, 0f, z));
                }
            }

            return spots.OrderBy(_ => random.Next()).ToList();
        }

        public static Quaternion AnyHeading(System.Random random)
        {
            return Quaternion.Euler(0f, (float)random.NextDouble() * FullTurnDegrees, 0f);
        }

        public static Quaternion SquareHeading(System.Random random)
        {
            return Quaternion.Euler(0f, random.Next(4) * QuarterTurnDegrees, 0f);
        }

        private static float Jitter(System.Random random, float cell)
        {
            return ((float)random.NextDouble() - Half) * cell * JitterShare;
        }
    }
}
