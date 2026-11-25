using System;
using Stilargo.Core.Input;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// The rules for turning and tilting the RTS camera around its focus: turn keys, and an orbit drag
    /// where horizontal mouse movement turns and vertical movement tilts.
    /// </summary>
    public sealed class CameraOrbit
    {
        private const float FullTurnDegrees = 360f;

        private readonly OrbitSettings _settings;

        public CameraOrbit(OrbitSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public CameraAngles Step(CameraAngles current, IPlayerInput input, float deltaTime)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            float yaw = current.YawDegrees + (input.TurnAxis * _settings.TurnDegreesPerSecond * deltaTime);
            float pitch = current.PitchDegrees;

            if (input.IsOrbitDragHeld)
            {
                yaw += input.PointerDelta.X * _settings.DragDegreesPerPixel;

                // Dragging up raises the camera's gaze towards the horizon, like pulling the view up.
                pitch -= input.PointerDelta.Y * _settings.DragDegreesPerPixel;
            }

            return Constrain(new CameraAngles(yaw, pitch));
        }

        /// <summary>Wraps yaw into [0, 360) and clamps pitch to the allowed range.</summary>
        public CameraAngles Constrain(CameraAngles angles)
        {
            float yaw = angles.YawDegrees % FullTurnDegrees;
            if (yaw < 0f)
            {
                yaw += FullTurnDegrees;
            }

            return new CameraAngles(yaw, _settings.Pitch.Clamp(angles.PitchDegrees));
        }
    }
}
