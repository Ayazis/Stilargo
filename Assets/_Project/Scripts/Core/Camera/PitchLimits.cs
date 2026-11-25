using System;

namespace Stilargo.Core.Camera
{
    /// <summary>How far the player may tilt the camera, in degrees down from the horizon.</summary>
    public sealed class PitchLimits
    {
        public const float LowestAllowedDegrees = 10f;
        public const float HighestAllowedDegrees = 90f;

        public PitchLimits(float minDegrees, float maxDegrees)
        {
            MinDegrees = RequireInRange(minDegrees, nameof(minDegrees));
            MaxDegrees = RequireInRange(maxDegrees, nameof(maxDegrees));

            if (MaxDegrees < MinDegrees)
            {
                throw new ArgumentException(
                    $"Max pitch ({maxDegrees}) must not be below min pitch ({minDegrees}).",
                    nameof(maxDegrees));
            }
        }

        public float MinDegrees { get; }

        public float MaxDegrees { get; }

        public float Clamp(float pitchDegrees)
        {
            return Math.Clamp(pitchDegrees, MinDegrees, MaxDegrees);
        }

        // Below 10 degrees the camera looks over the horizon and the focus point is far off screen.
        private static float RequireInRange(float degrees, string name)
        {
            if (float.IsFinite(degrees) && degrees >= LowestAllowedDegrees && degrees <= HighestAllowedDegrees)
            {
                return degrees;
            }

            throw new ArgumentOutOfRangeException(
                name,
                degrees,
                $"Pitch must be between {LowestAllowedDegrees} and {HighestAllowedDegrees} degrees.");
        }
    }
}
