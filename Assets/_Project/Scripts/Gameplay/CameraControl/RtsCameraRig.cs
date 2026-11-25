using System;
using Stilargo.Core.Camera;
using Stilargo.Core.Input;
using UnityEngine;

namespace Stilargo.Gameplay.CameraControl
{
    /// <summary>
    /// Drives the transform it sits on (the camera) from the Core RTS camera rules: pan and zoom from
    /// <see cref="RtsCameraMotion"/>, turn and tilt from <see cref="CameraOrbit"/>.
    /// Does nothing until the composition root calls <see cref="Construct"/>.
    /// </summary>
    public sealed class RtsCameraRig : MonoBehaviour
    {
        private IPlayerInput _input;
        private RtsCameraMotion _motion;
        private CameraOrbit _orbit;
        private CameraState _state;
        private CameraAngles _angles;

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

            _input = input;
            _motion = new RtsCameraMotion(settings.ToMotionSettings());
            _orbit = new CameraOrbit(settings.ToOrbitSettings());
            _angles = _orbit.Constrain(new CameraAngles(transform.eulerAngles.y, settings.PitchDegrees));

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
            float deltaTime = Time.unscaledDeltaTime;
            _angles = _orbit.Step(_angles, _input, deltaTime);
            var turned = new CameraState(_state.Focus, _state.Height, _angles.YawDegrees);
            _state = _motion.Step(turned, _input, new CameraFrame(new ScreenSize(Screen.width, Screen.height), deltaTime));
            ApplyPose();
        }

        private CameraState StateFromTransform()
        {
            Vector3 position = transform.position;
            Vector3 focus = position + (FlatForward() * HorizontalOffset(position.y));
            return new CameraState(focus.ToGroundPlane(), position.y, _angles.YawDegrees);
        }

        private void ApplyPose()
        {
            // The camera sits behind the focus, against its facing, so that looking down at the pitch
            // the focus ends up in the centre of the screen.
            Vector3 focus = _state.Focus.ToWorldPosition(0f);
            Vector3 position = focus - (FlatForward() * HorizontalOffset(_state.Height)) + (Vector3.up * _state.Height);
            transform.SetPositionAndRotation(position, Quaternion.Euler(_angles.PitchDegrees, _angles.YawDegrees, 0f));
        }

        private Vector3 FlatForward()
        {
            return Quaternion.Euler(0f, _angles.YawDegrees, 0f) * Vector3.forward;
        }

        /// <summary>Ground distance between the camera and its focus for a given height.</summary>
        private float HorizontalOffset(float height)
        {
            float pitchRadians = _angles.PitchDegrees * Mathf.Deg2Rad;

            // Cos over sin rather than 1 / tan, which blows up to a huge negative number at 90 degrees.
            return height * Mathf.Cos(pitchRadians) / Mathf.Sin(pitchRadians);
        }
    }
}
