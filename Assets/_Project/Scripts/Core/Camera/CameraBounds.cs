using System;
using System.Numerics;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// The ground rectangle the camera focus may move in. X is world X, Y is world Z.
    /// </summary>
    public sealed class CameraBounds
    {
        public CameraBounds(Vector2 min, Vector2 max)
        {
            SettingGuard.Finite(min.X, nameof(min));
            SettingGuard.Finite(min.Y, nameof(min));
            SettingGuard.Finite(max.X, nameof(max));
            SettingGuard.Finite(max.Y, nameof(max));

            if (max.X < min.X || max.Y < min.Y)
            {
                throw new ArgumentException($"Bounds max {max} must not be below min {min} on any axis.", nameof(max));
            }

            Min = min;
            Max = max;
        }

        public Vector2 Min { get; }

        public Vector2 Max { get; }

        public Vector2 Clamp(Vector2 point)
        {
            return Vector2.Clamp(point, Min, Max);
        }
    }
}
