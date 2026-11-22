using System;
using Stilargo.Core.Combat;

namespace Stilargo.Core.Cover
{
    /// <summary>Tuning for cover: placement, how much it protects and how close a unit must stand.</summary>
    public sealed class CoverSettings
    {
        public const float DefaultHitChanceMultiplier = 0.5f;
        public const float DefaultCaptureRadiusMetres = 1.2f;

        private const float NoDamageReduction = 1f;

        public CoverSettings(CoverPlacement placement, float hitChanceMultiplier, float captureRadiusMetres)
        {
            if (!(hitChanceMultiplier >= 0f && hitChanceMultiplier < 1f))
            {
                throw new ArgumentOutOfRangeException(nameof(hitChanceMultiplier), hitChanceMultiplier, "Cover hit-chance multiplier must be in [0, 1).");
            }

            if (!(captureRadiusMetres > 0f) || float.IsInfinity(captureRadiusMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(captureRadiusMetres), captureRadiusMetres, "Capture radius must be a positive finite number.");
            }

            Placement = placement ?? throw new ArgumentNullException(nameof(placement));
            Protection = new CombatProtection(hitChanceMultiplier, NoDamageReduction);
            CaptureRadiusMetres = captureRadiusMetres;
        }

        public static CoverSettings Default => new CoverSettings(CoverPlacement.Default, DefaultHitChanceMultiplier, DefaultCaptureRadiusMetres);

        public CoverPlacement Placement { get; }

        /// <summary>Protection granted in cover: reduced hit chance, full damage.</summary>
        public CombatProtection Protection { get; }

        public float CaptureRadiusMetres { get; }
    }
}
