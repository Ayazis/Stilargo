using System;

namespace Stilargo.Core.Garrison
{
    /// <summary>How garrison slots are laid out on a building. Immutable; slot counts change via the With... methods.</summary>
    public sealed class GarrisonLayoutSettings
    {
        public const float DefaultStoreyHeightMetres = 3f;
        public const float DefaultWindowOffsetMetres = 0.5f;
        public const int DefaultRoofSlotCount = 4;
        public const int DefaultWindowsPerFacadePerStorey = 2;

        public GarrisonLayoutSettings()
            : this(DefaultStoreyHeightMetres, DefaultWindowOffsetMetres)
        {
        }

        public GarrisonLayoutSettings(float storeyHeightMetres, float windowOffsetMetres)
        {
            if (!(storeyHeightMetres > 0f) || float.IsInfinity(storeyHeightMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(storeyHeightMetres), storeyHeightMetres, "Storey height must be positive and finite.");
            }

            if (!(windowOffsetMetres >= 0f) || float.IsInfinity(windowOffsetMetres))
            {
                throw new ArgumentOutOfRangeException(nameof(windowOffsetMetres), windowOffsetMetres, "Window offset must be non-negative and finite.");
            }

            StoreyHeightMetres = storeyHeightMetres;
            WindowOffsetMetres = windowOffsetMetres;
            RoofSlotCount = DefaultRoofSlotCount;
            WindowsPerFacadePerStorey = DefaultWindowsPerFacadePerStorey;
        }

        public float StoreyHeightMetres { get; }

        /// <summary>How far outside the facade a window slot sits.</summary>
        public float WindowOffsetMetres { get; }

        public int RoofSlotCount { get; private set; }

        /// <summary>Window slots on each road-facing facade, per storey.</summary>
        public int WindowsPerFacadePerStorey { get; private set; }

        public GarrisonLayoutSettings WithRoofSlotCount(int roofSlotCount)
        {
            RequireNonNegative(roofSlotCount, nameof(roofSlotCount));
            GarrisonLayoutSettings copy = Copy();
            copy.RoofSlotCount = roofSlotCount;
            return copy;
        }

        public GarrisonLayoutSettings WithWindowsPerFacadePerStorey(int windowsPerFacadePerStorey)
        {
            RequireNonNegative(windowsPerFacadePerStorey, nameof(windowsPerFacadePerStorey));
            GarrisonLayoutSettings copy = Copy();
            copy.WindowsPerFacadePerStorey = windowsPerFacadePerStorey;
            return copy;
        }

        private GarrisonLayoutSettings Copy()
        {
            return (GarrisonLayoutSettings)MemberwiseClone();
        }

        private static void RequireNonNegative(int value, string name)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(name, value, "Slot count must not be negative.");
            }
        }
    }
}
