using System;

namespace Stilargo.Core.Hill
{
    /// <summary>
    /// Tunable values for the hill, loaded from <c>Hill.json</c>. Everything is validated on construction, so an
    /// invalid file fails at startup with a clear message. Positions are world metres, city corner at the origin.
    /// </summary>
    public sealed class HillSettings
    {
        // A road stretch about as far from the player's home as from the rival's start in the default city.
        public const float DefaultCenterXMetres = 86f;
        public const float DefaultCenterZMetres = 72f;
        public const float DefaultRadiusMetres = 6f;
        public const float DefaultHoldDurationSeconds = 180f;

        public HillSettings(float centerXMetres, float centerZMetres, float radiusMetres)
        {
            CenterXMetres = RequireFinite(centerXMetres, nameof(centerXMetres));
            CenterZMetres = RequireFinite(centerZMetres, nameof(centerZMetres));
            RadiusMetres = RequirePositive(radiusMetres, nameof(radiusMetres));
        }

        public static HillSettings Default => new HillSettings(DefaultCenterXMetres, DefaultCenterZMetres, DefaultRadiusMetres)
            .WithHoldDuration(DefaultHoldDurationSeconds);

        public float CenterXMetres { get; }

        public float CenterZMetres { get; }

        public float RadiusMetres { get; }

        /// <summary>Total time the player must hold the hill to win; separate holds add up.</summary>
        public float HoldDurationSeconds { get; private set; } = DefaultHoldDurationSeconds;

        public HillSettings WithHoldDuration(float seconds)
        {
            HoldDurationSeconds = RequirePositive(seconds, nameof(seconds));
            return this;
        }

        private static float RequireFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be a finite number.");
            }

            return value;
        }

        private static float RequirePositive(float value, string name)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be positive and finite.");
            }

            return value;
        }
    }
}
