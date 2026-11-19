using Stilargo.Core.Camera;
using UnityEngine;

namespace Stilargo.Gameplay.CameraControl
{
    /// <summary>
    /// Designer-tunable RTS camera values. Validation happens in Core when converted,
    /// so a bad asset fails as soon as the camera is constructed.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Stilargo/Camera Settings")]
    public sealed class CameraSettings : ScriptableObject
    {
        public const float MinPitchDegrees = 10f;
        public const float MaxPitchDegrees = 90f;

        private const float DefaultPanSpeedPerHeight = 1.5f;
        private const float DefaultEdgeScrollMarginPixels = 12f;
        private const float DefaultHeightPerZoomStep = 4f;
        private const float DefaultMinHeight = 10f;
        private const float DefaultMaxHeight = 60f;
        private const float DefaultBoundsExtent = 100f;
        private const float DefaultPitchDegrees = 60f;

        [Header("Pan")]
        [Tooltip("World units per second for each unit of camera height, so panning feels the same at every zoom.")]
        [SerializeField] private float _panSpeedPerHeight = DefaultPanSpeedPerHeight;

        [Tooltip("Width in pixels of the screen-edge band that scrolls the camera. Zero disables edge scroll.")]
        [SerializeField] private float _edgeScrollMarginPixels = DefaultEdgeScrollMarginPixels;

        [Header("Zoom")]
        [Tooltip("Height change for one unit of zoom input.")]
        [SerializeField] private float _heightPerZoomStep = DefaultHeightPerZoomStep;

        [SerializeField] private float _minHeight = DefaultMinHeight;

        [SerializeField] private float _maxHeight = DefaultMaxHeight;

        [Header("Map bounds (world X, world Z)")]
        [SerializeField] private Vector2 _boundsMin = new Vector2(-DefaultBoundsExtent, -DefaultBoundsExtent);

        [SerializeField] private Vector2 _boundsMax = new Vector2(DefaultBoundsExtent, DefaultBoundsExtent);

        [Header("View")]
        [Tooltip("Degrees the camera looks down from the horizon. 90 is straight down.")]
        [Range(MinPitchDegrees, MaxPitchDegrees)]
        [SerializeField] private float _pitchDegrees = DefaultPitchDegrees;

        public float PitchDegrees => _pitchDegrees;

        public CameraMotionSettings ToMotionSettings()
        {
            return new CameraMotionSettings(
                new PanSettings(_panSpeedPerHeight, _edgeScrollMarginPixels),
                new ZoomSettings(_heightPerZoomStep, _minHeight, _maxHeight),
                new CameraBounds(_boundsMin.ToNumerics(), _boundsMax.ToNumerics()));
        }
    }
}
