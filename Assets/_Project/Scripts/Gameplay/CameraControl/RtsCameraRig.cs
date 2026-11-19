using System;
using Stilargo.Core.Camera;
using Stilargo.Core.Input;
using UnityEngine;

namespace Stilargo.Gameplay.CameraControl
{
    /// <summary>
    /// Drives the transform it sits on (the camera) from the Core RTS camera rules.
    /// Does nothing until the composition root calls <see cref="Construct"/>.
    /// </summary>
    public sealed class RtsCameraRig : MonoBehaviour
    {
        private IPlayerInput _input;
        private RtsCameraMotion _motion;
        private CameraState _state;
        private float _pitchDegrees;

        private bool IsConstructed => _motion != null;

        public void Construct(IPlayerInput input, CameraSettings settings)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            RequireValidPitch(settings);

            _input = input;
            _pitchDegrees = settings.PitchDegrees;
            _motion = new RtsCameraMotion(settings.ToMotionSettings());

            // Start from wherever the camera was placed in the scene, pulled inside the map.
            _state = _motion.Constrain(StateFromTransform());
            ApplyPose();
        }

        private void LateUpdate()
        {
            if (!IsConstructed)
            {
                return;
            }

            // Unscaled time so the camera still moves while the game is paused or slowed.
            var frame = new CameraFrame(new ScreenSize(Screen.width, Screen.height), Time.unscaledDeltaTime);
            _state = _motion.Step(_state, _input, frame);
            ApplyPose();
        }

        private static void RequireValidPitch(CameraSettings settings)
        {
            float pitch = settings.PitchDegrees;
            if (pitch >= CameraSettings.MinPitchDegrees && pitch <= CameraSettings.MaxPitchDegrees)
            {
                return;
            }

            throw new ArgumentOutOfRangeException(
                nameof(settings),
                pitch,
                $"Camera pitch must be between {CameraSettings.MinPitchDegrees} and {CameraSettings.MaxPitchDegrees} degrees.");
        }

        private CameraState StateFromTransform()
        {
            Vector3 position = transform.position;
            Vector3 focus = position + new Vector3(0f, 0f, HorizontalOffset(position.y));
            return new CameraState(focus.ToGroundPlane(), position.y);
        }

        private void ApplyPose()
        {
            // The camera sits behind the focus (towards -Z) so that, looking down at the pitch,
            // the focus ends up in the centre of the screen.
            Vector3 focus = _state.Focus.ToWorldPosition(0f);
            Vector3 position = focus + new Vector3(0f, _state.Height, -HorizontalOffset(_state.Height));
            transform.SetPositionAndRotation(position, Quaternion.Euler(_pitchDegrees, 0f, 0f));
        }

        /// <summary>Ground distance between the camera and its focus for a given height.</summary>
        private float HorizontalOffset(float height)
        {
            float pitchRadians = _pitchDegrees * Mathf.Deg2Rad;

            // Cos over sin rather than 1 / tan, which blows up to a huge negative number at 90 degrees.
            return height * Mathf.Cos(pitchRadians) / Mathf.Sin(pitchRadians);
        }
    }
}
