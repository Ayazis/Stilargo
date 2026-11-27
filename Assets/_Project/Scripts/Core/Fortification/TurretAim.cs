using System;
using System.Numerics;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Where a turret's gun points, as yaw (left/right) and pitch (up/down) in degrees from its rest direction.
    /// Each step swings it toward a target at limited speed, so the gun visibly tracks instead of snapping.
    /// Directions are in the turret's own frame: +Z is its rest forward, +Y up, +X right.
    /// </summary>
    public sealed class TurretAim
    {
        private const float RadiansToDegrees = 180f / MathF.PI;
        private const float FullTurnDegrees = 360f;
        private const float HalfTurnDegrees = 180f;

        private readonly TurretAimSettings _settings;

        public TurretAim(TurretAimSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public float YawDegrees { get; private set; }

        public float PitchDegrees { get; private set; }

        public void TurnToward(Vector3 localDirection, float deltaSeconds)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds), deltaSeconds, "Time step must not be negative.");
            }

            float horizontal = MathF.Sqrt((localDirection.X * localDirection.X) + (localDirection.Z * localDirection.Z));
            if (horizontal <= float.Epsilon && MathF.Abs(localDirection.Y) <= float.Epsilon)
            {
                return;
            }

            float targetYaw = MathF.Atan2(localDirection.X, localDirection.Z) * RadiansToDegrees;
            float targetPitch = Math.Clamp(MathF.Atan2(localDirection.Y, horizontal) * RadiansToDegrees, _settings.MinPitchDegrees, _settings.MaxPitchDegrees);

            float yawStep = ShortestTurn(YawDegrees, targetYaw);
            YawDegrees = Wrap(YawDegrees + StepToward(yawStep, _settings.YawDegreesPerSecond * deltaSeconds));
            PitchDegrees += StepToward(targetPitch - PitchDegrees, _settings.PitchDegreesPerSecond * deltaSeconds);
        }

        private static float StepToward(float remaining, float maxStep)
        {
            return Math.Clamp(remaining, -maxStep, maxStep);
        }

        private static float ShortestTurn(float from, float to)
        {
            return Wrap(to - from);
        }

        // Keeps angles in (-180, 180] so the gun always turns the short way round.
        private static float Wrap(float degrees)
        {
            float wrapped = degrees % FullTurnDegrees;
            if (wrapped > HalfTurnDegrees)
            {
                return wrapped - FullTurnDegrees;
            }

            return wrapped <= -HalfTurnDegrees ? wrapped + FullTurnDegrees : wrapped;
        }
    }
}
