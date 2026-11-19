using System;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Rejects bad tuning values at construction so a broken asset fails loudly
    /// instead of producing a camera that silently drifts or never moves.
    /// </summary>
    internal static class SettingGuard
    {
        public static float Positive(float value, string name)
        {
            if (float.IsFinite(value) && value > 0f)
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(name, value, "Must be a finite number greater than zero.");
        }

        public static float NonNegative(float value, string name)
        {
            if (float.IsFinite(value) && value >= 0f)
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(name, value, "Must be a finite number of zero or more.");
        }

        public static float Finite(float value, string name)
        {
            if (float.IsFinite(value))
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(name, value, "Must be a finite number.");
        }
    }
}
