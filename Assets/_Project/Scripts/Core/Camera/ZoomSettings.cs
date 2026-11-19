using System;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// How much one unit of zoom input changes the camera height, and the allowed height range.
    /// </summary>
    public sealed class ZoomSettings
    {
        public ZoomSettings(float heightPerZoomStep, float minHeight, float maxHeight)
        {
            HeightPerZoomStep = SettingGuard.Positive(heightPerZoomStep, nameof(heightPerZoomStep));
            MinHeight = SettingGuard.Positive(minHeight, nameof(minHeight));
            MaxHeight = SettingGuard.Positive(maxHeight, nameof(maxHeight));

            if (MaxHeight < MinHeight)
            {
                throw new ArgumentException(
                    $"Max height ({maxHeight}) must not be below min height ({minHeight}).",
                    nameof(maxHeight));
            }
        }

        public float HeightPerZoomStep { get; }

        public float MinHeight { get; }

        public float MaxHeight { get; }
    }
}
