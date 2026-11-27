using System;

namespace Stilargo.Core.Fortification
{
    /// <summary>How fast a turret swings toward its target and how far its barrels tilt. Loaded from <c>Fortification.json</c>.</summary>
    public sealed class TurretAimSettings
    {
        public const float DefaultYawDegreesPerSecond = 120f;
        public const float DefaultPitchDegreesPerSecond = 60f;
        public const float DefaultMinPitchDegrees = -15f;
        public const float DefaultMaxPitchDegrees = 60f;

        public TurretAimSettings(float yawDegreesPerSecond, float pitchDegreesPerSecond, float minPitchDegrees, float maxPitchDegrees)
        {
            RequirePositive(yawDegreesPerSecond, nameof(yawDegreesPerSecond));
            RequirePositive(pitchDegreesPerSecond, nameof(pitchDegreesPerSecond));
            if (!(minPitchDegrees >= -90f && minPitchDegrees < maxPitchDegrees && maxPitchDegrees <= 90f))
            {
                throw new ArgumentException($"Pitch limits must satisfy -90 <= min < max <= 90 (got {minPitchDegrees} to {maxPitchDegrees}).");
            }

            YawDegreesPerSecond = yawDegreesPerSecond;
            PitchDegreesPerSecond = pitchDegreesPerSecond;
            MinPitchDegrees = minPitchDegrees;
            MaxPitchDegrees = maxPitchDegrees;
        }

        public static TurretAimSettings Default => new TurretAimSettings(
            DefaultYawDegreesPerSecond, DefaultPitchDegreesPerSecond, DefaultMinPitchDegrees, DefaultMaxPitchDegrees);

        public float YawDegreesPerSecond { get; }

        public float PitchDegreesPerSecond { get; }

        public float MinPitchDegrees { get; }

        public float MaxPitchDegrees { get; }

        private static void RequirePositive(float value, string name)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, $"{name} must be positive and finite.");
            }
        }
    }
}
