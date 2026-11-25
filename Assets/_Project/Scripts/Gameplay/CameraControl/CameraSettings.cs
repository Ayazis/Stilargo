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
        public const float MinPitchDegrees = PitchLimits.LowestAllowedDegrees;
        public const float MaxPitchDegrees = PitchLimits.HighestAllowedDegrees;

        private const float DefaultPanSpeedPerHeight = 1.5f;
        private const float DefaultEdgeScrollMarginPixels = 12f;
        private const float DefaultHeightPerZoomStep = 4f;
        private const float DefaultMinHeight = 10f;
        private const float DefaultMaxHeight = 60f;
        private const float DefaultBoundsExtent = 100f;
        private const float DefaultPitchDegrees = 60f;
        private const float DefaultTurnDegreesPerSecond = 90f;
        private const float DefaultDragDegreesPerPixel = 0.25f;
        private const float DefaultMinTiltDegrees = 25f;
        private const float DefaultMaxTiltDegrees = 85f;

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
        [Tooltip("Starting degrees the camera looks down from the horizon. 90 is straight down.")]
        [Range(MinPitchDegrees, MaxPitchDegrees)]
        [SerializeField] private float _pitchDegrees = DefaultPitchDegrees;

        [Header("Turn and tilt")]
        [Tooltip("Turn speed while Q or E is held.")]
        [SerializeField] private float _turnDegreesPerSecond = DefaultTurnDegreesPerSecond;

        [Tooltip("Turn and tilt per pixel of mouse movement while the middle button is held.")]
        [SerializeField] private float _dragDegreesPerPixel = DefaultDragDegreesPerPixel;

        [Range(MinPitchDegrees, MaxPitchDegrees)]
        [SerializeField] private float _minTiltDegrees = DefaultMinTiltDegrees;

        [Range(MinPitchDegrees, MaxPitchDegrees)]
        [SerializeField] private float _maxTiltDegrees = DefaultMaxTiltDegrees;

        public float PitchDegrees => _pitchDegrees;

        public OrbitSettings ToOrbitSettings()
        {
            return new OrbitSettings(
                _turnDegreesPerSecond,
                _dragDegreesPerPixel,
                new PitchLimits(_minTiltDegrees, _maxTiltDegrees));
        }

        public CameraMotionSettings ToMotionSettings()
        {
            return new CameraMotionSettings(
                new PanSettings(_panSpeedPerHeight, _edgeScrollMarginPixels),
                new ZoomSettings(_heightPerZoomStep, _minHeight, _maxHeight),
                new CameraBounds(_boundsMin.ToNumerics(), _boundsMax.ToNumerics()));
        }
    }
}
