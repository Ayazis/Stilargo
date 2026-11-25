using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Builds a hip roof whose four slopes share one pitch, with the ridge along X.
    /// UV u runs along the eaves and v up the slope, both in metres, so tile textures never stretch
    /// and their rows always run down the slope.
    /// </summary>
    public static class HipRoofBuilder
    {
        private const float Half = 0.5f;

        public static RoofGeometry Build(float length, float width, float rise)
        {
            RequirePositive(length, nameof(length));
            RequirePositive(width, nameof(width));
            RequirePositive(rise, nameof(rise));
            if (width > length)
            {
                throw new ArgumentException($"Width {width} exceeds length {length}; turn the roof so its ridge runs along the longer side.");
            }

            float halfLength = length * Half;
            float halfWidth = width * Half;
            float halfRidge = halfLength - halfWidth;
            float slope = MathF.Sqrt((halfWidth * halfWidth) + (rise * rise));

            var southWest = new Vector3(-halfLength, 0f, -halfWidth);
            var southEast = new Vector3(halfLength, 0f, -halfWidth);
            var northEast = new Vector3(halfLength, 0f, halfWidth);
            var northWest = new Vector3(-halfLength, 0f, halfWidth);
            var ridgeWest = new Vector3(-halfRidge, rise, 0f);
            var ridgeEast = new Vector3(halfRidge, rise, 0f);

            var vertices = new List<Vector3>
            {
                southWest, ridgeWest, ridgeEast, southEast,
                northEast, ridgeEast, ridgeWest, northWest,
                southEast, ridgeEast, northEast,
                northWest, ridgeWest, southWest,
            };
            var uvs = new List<Vector2>
            {
                new Vector2(-halfLength, 0f), new Vector2(-halfRidge, slope), new Vector2(halfRidge, slope), new Vector2(halfLength, 0f),
                new Vector2(-halfLength, 0f), new Vector2(-halfRidge, slope), new Vector2(halfRidge, slope), new Vector2(halfLength, 0f),
                new Vector2(-halfWidth, 0f), new Vector2(0f, slope), new Vector2(halfWidth, 0f),
                new Vector2(-halfWidth, 0f), new Vector2(0f, slope), new Vector2(halfWidth, 0f),
            };
            var triangles = new List<int>
            {
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                8, 9, 10,
                11, 12, 13,
            };
            return new RoofGeometry(vertices, uvs, triangles);
        }

        private static void RequirePositive(float value, string name)
        {
            if (value <= 0f)
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be positive.");
            }
        }
    }
}
