using System;
using System.Numerics;
using Stilargo.Core.Input;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// The rules for moving a top-down RTS camera: keyboard and edge-scroll pan,
    /// scroll-wheel zoom, and clamping to the map and the height range.
    /// </summary>
    public sealed class RtsCameraMotion
    {
        private const float MaxPanDirectionLength = 1f;

        private readonly CameraMotionSettings _settings;
        private readonly EdgeScroll _edgeScroll;

        public RtsCameraMotion(CameraMotionSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _edgeScroll = new EdgeScroll(settings.Pan.EdgeMarginPixels);
        }

        public CameraState Step(CameraState current, IPlayerInput input, CameraFrame frame)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            float height = ClampHeight(current.Height - (input.ZoomDelta * _settings.Zoom.HeightPerZoomStep));
            Vector2 direction = PanDirection(input, frame.Screen);
            float speed = _settings.Pan.SpeedPerHeight * height;
            Vector2 focus = current.Focus + (direction * speed * frame.DeltaTime);

            return new CameraState(_settings.Bounds.Clamp(focus), height);
        }

        /// <summary>Pulls a state (for example the one placed in the scene) inside the bounds and height range.</summary>
        public CameraState Constrain(CameraState state)
        {
            return new CameraState(_settings.Bounds.Clamp(state.Focus), ClampHeight(state.Height));
        }

        private Vector2 PanDirection(IPlayerInput input, ScreenSize screen)
        {
            Vector2 combined = input.PanAxis + _edgeScroll.DirectionFor(input.PointerScreenPosition, screen);
            float length = combined.Length();

            // Diagonals and keyboard plus edge scroll together must not pan faster than one key.
            if (length <= MaxPanDirectionLength)
            {
                return combined;
            }

            return combined / length;
        }

        private float ClampHeight(float height)
        {
            return Math.Clamp(height, _settings.Zoom.MinHeight, _settings.Zoom.MaxHeight);
        }
    }
}
