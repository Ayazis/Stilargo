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
        private const float DegreesToRadians = MathF.PI / 180f;

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
            Vector2 direction = ToWorld(PanDirection(input, frame.Screen), current.YawDegrees);
            float speed = _settings.Pan.SpeedPerHeight * height;
            Vector2 focus = current.Focus + (direction * speed * frame.DeltaTime);

            return new CameraState(_settings.Bounds.Clamp(focus), height, current.YawDegrees);
        }

        /// <summary>Pulls a state (for example the one placed in the scene) inside the bounds and height range.</summary>
        public CameraState Constrain(CameraState state)
        {
            return new CameraState(_settings.Bounds.Clamp(state.Focus), ClampHeight(state.Height), state.YawDegrees);
        }

        /// <summary>Turns a screen-relative direction (X right, Y up the screen) into ground X/Z for the given yaw.</summary>
        private static Vector2 ToWorld(Vector2 screenDirection, float yawDegrees)
        {
            float yaw = yawDegrees * DegreesToRadians;
            var right = new Vector2(MathF.Cos(yaw), -MathF.Sin(yaw));
            var forward = new Vector2(MathF.Sin(yaw), MathF.Cos(yaw));
            return (screenDirection.X * right) + (screenDirection.Y * forward);
        }

        private Vector2 PanDirection(IPlayerInput input, ScreenSize screen)
        {
            Vector2 edge = input.IsEdgeScrollEnabled ? _edgeScroll.DirectionFor(input.PointerScreenPosition, screen) : Vector2.Zero;
            Vector2 combined = input.PanAxis + edge;
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
