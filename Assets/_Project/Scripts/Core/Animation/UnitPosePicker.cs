using System;

namespace Stilargo.Core.Animation
{
    /// <summary>
    /// Picks a unit's pose: moving beats everything, then cover (crouch), then a recent shot (aim), else idle.
    /// </summary>
    public sealed class UnitPosePicker
    {
        private readonly float _runThresholdMetresPerSecond;
        private readonly float _aimHoldSeconds;

        public UnitPosePicker(float runThresholdMetresPerSecond, float aimHoldSeconds)
        {
            if (!(runThresholdMetresPerSecond > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(runThresholdMetresPerSecond), runThresholdMetresPerSecond, "Must be positive.");
            }

            if (!(aimHoldSeconds >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(aimHoldSeconds), aimHoldSeconds, "Must not be negative.");
            }

            _runThresholdMetresPerSecond = runThresholdMetresPerSecond;
            _aimHoldSeconds = aimHoldSeconds;
        }

        public UnitPose Pick(UnitPoseInput input)
        {
            if (input.SpeedMetresPerSecond >= _runThresholdMetresPerSecond)
            {
                return UnitPose.Run;
            }

            if (input.IsInCover)
            {
                return UnitPose.Crouch;
            }

            return input.SecondsSinceShot <= _aimHoldSeconds ? UnitPose.Aim : UnitPose.Idle;
        }
    }
}
