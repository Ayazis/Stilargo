namespace Stilargo.Core.Animation
{
    /// <summary>What a unit is doing this frame, as far as its pose cares.</summary>
    public readonly struct UnitPoseInput
    {
        public UnitPoseInput(float speedMetresPerSecond, float secondsSinceShot, bool isInCover)
        {
            SpeedMetresPerSecond = speedMetresPerSecond;
            SecondsSinceShot = secondsSinceShot;
            IsInCover = isInCover;
        }

        public float SpeedMetresPerSecond { get; }

        public float SecondsSinceShot { get; }

        public bool IsInCover { get; }
    }
}
