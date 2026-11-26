using System;

namespace Stilargo.Core.Fog
{
    /// <summary>
    /// Tunable values for how far units see, loaded from <c>Vision.json</c>. Buildings block the view, so the base
    /// radius can be generous: it mostly reaches along open streets. Height adds range, so a rooftop or the church
    /// tower overlooks a wide area.
    /// </summary>
    public sealed class VisionSettings
    {
        public const float DefaultUnitVisionRadiusMetres = 24f;
        public const float DefaultEyeHeightMetres = 1.7f;
        public const float DefaultSightMetresPerMetreOfHeight = 1.5f;
        public const float DefaultMaxVisionRadiusMetres = 60f;

        /// <summary>How far a soldier on the street sees where nothing is in the way.</summary>
        public float UnitVisionRadiusMetres { get; private set; } = DefaultUnitVisionRadiusMetres;

        public float EyeHeightMetres { get; private set; } = DefaultEyeHeightMetres;

        /// <summary>Extra vision radius for each metre a unit stands above the street.</summary>
        public float SightMetresPerMetreOfHeight { get; private set; } = DefaultSightMetresPerMetreOfHeight;

        /// <summary>No source sees further than this, however high it stands.</summary>
        public float MaxVisionRadiusMetres { get; private set; } = DefaultMaxVisionRadiusMetres;

        public VisionSettings WithUnitVision(float radiusMetres, float eyeHeightMetres)
        {
            UnitVisionRadiusMetres = RequirePositive(radiusMetres, nameof(radiusMetres));
            EyeHeightMetres = RequirePositive(eyeHeightMetres, nameof(eyeHeightMetres));
            return this;
        }

        public VisionSettings WithHeightBonus(float sightMetresPerMetreOfHeight, float maxVisionRadiusMetres)
        {
            if (!(sightMetresPerMetreOfHeight >= 0f) || float.IsInfinity(sightMetresPerMetreOfHeight))
            {
                throw new ArgumentOutOfRangeException(nameof(sightMetresPerMetreOfHeight), sightMetresPerMetreOfHeight, "Must be a non-negative number.");
            }

            SightMetresPerMetreOfHeight = sightMetresPerMetreOfHeight;
            MaxVisionRadiusMetres = RequirePositive(maxVisionRadiusMetres, nameof(maxVisionRadiusMetres));
            return this;
        }

        /// <summary>The vision radius of a unit whose feet are <paramref name="heightAboveStreetMetres"/> above the street.</summary>
        public float RadiusAt(float heightAboveStreetMetres, float bonusMetres)
        {
            float height = Math.Max(0f, heightAboveStreetMetres);
            float radius = UnitVisionRadiusMetres + bonusMetres + (height * SightMetresPerMetreOfHeight);
            return Math.Min(MaxVisionRadiusMetres, Math.Max(0f, radius));
        }

        private static float RequirePositive(float value, string name)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be a positive number.");
            }

            return value;
        }
    }
}
