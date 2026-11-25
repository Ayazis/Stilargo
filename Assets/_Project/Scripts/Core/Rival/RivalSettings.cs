using System;

namespace Stilargo.Core.Rival
{
    /// <summary>
    /// Tunable values for the AI rival, loaded from <c>Hill.json</c>. Defaults match the shipped file; every
    /// setter validates, so an invalid file fails at startup with a clear message.
    /// </summary>
    public sealed class RivalSettings
    {
        public const int DefaultSquadSize = 4;
        public const float DefaultMoveSpeedMetresPerSecond = 6f;
        public const float DefaultScavengeSeconds = 45f;
        public const float DefaultContestSeconds = 40f;
        public const float DefaultRaidSeconds = 30f;
        public const float DefaultScavengeReachMetres = 2.5f;
        public const float DefaultScavengeRadiusMetres = 30f;
        public const float DefaultHillSpreadMetres = 2f;
        public const float DefaultArrivalToleranceMetres = 2f;

        /// <summary>How many of the scene's enemy units the rival commands, taken in scene order.</summary>
        public int SquadSize { get; private set; } = DefaultSquadSize;

        public float MoveSpeedMetresPerSecond { get; private set; } = DefaultMoveSpeedMetresPerSecond;

        /// <summary>How long a scavenging run lasts before the rival heads for the hill.</summary>
        public float ScavengeSeconds { get; private set; } = DefaultScavengeSeconds;

        /// <summary>The least time the rival stays on the hill before scavenging or raiding.</summary>
        public float ContestSeconds { get; private set; } = DefaultContestSeconds;

        /// <summary>How long a raid on the claimed house lasts before the rival returns to the hill.</summary>
        public float RaidSeconds { get; private set; } = DefaultRaidSeconds;

        /// <summary>How close a rival unit must get to a cache to pick it up, or to its base to deliver.</summary>
        public float ScavengeReachMetres { get; private set; } = DefaultScavengeReachMetres;

        /// <summary>
        /// The rival only loots caches this close to its base, so it doesn't walk into the player's home by accident.
        /// </summary>
        public float ScavengeRadiusMetres { get; private set; } = DefaultScavengeRadiusMetres;

        /// <summary>Radius of the circle the squad spreads over around the hill's centre.</summary>
        public float HillSpreadMetres { get; private set; } = DefaultHillSpreadMetres;

        /// <summary>A unit this close to where it is sent counts as already there and gets no new order.</summary>
        public float ArrivalToleranceMetres { get; private set; } = DefaultArrivalToleranceMetres;

        public RivalSettings WithSquadSize(int units)
        {
            if (units <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(units), units, "Squad size must be at least 1.");
            }

            SquadSize = units;
            return this;
        }

        public RivalSettings WithMoveSpeed(float metresPerSecond)
        {
            MoveSpeedMetresPerSecond = RequirePositive(metresPerSecond, nameof(metresPerSecond));
            return this;
        }

        public RivalSettings WithStrategyTimes(float scavengeSeconds, float contestSeconds, float raidSeconds)
        {
            ScavengeSeconds = RequirePositive(scavengeSeconds, nameof(scavengeSeconds));
            ContestSeconds = RequirePositive(contestSeconds, nameof(contestSeconds));
            RaidSeconds = RequirePositive(raidSeconds, nameof(raidSeconds));
            return this;
        }

        public RivalSettings WithScavengeReach(float metres)
        {
            ScavengeReachMetres = RequirePositive(metres, nameof(metres));
            return this;
        }

        public RivalSettings WithScavengeRadius(float metres)
        {
            ScavengeRadiusMetres = RequirePositive(metres, nameof(metres));
            return this;
        }

        public RivalSettings WithHillSpread(float metres)
        {
            if (!(metres >= 0f) || float.IsInfinity(metres))
            {
                throw new ArgumentOutOfRangeException(nameof(metres), metres, "Hill spread must be zero or more and finite.");
            }

            HillSpreadMetres = metres;
            return this;
        }

        public RivalSettings WithArrivalTolerance(float metres)
        {
            ArrivalToleranceMetres = RequirePositive(metres, nameof(metres));
            return this;
        }

        private static float RequirePositive(float value, string name)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Must be positive and finite.");
            }

            return value;
        }
    }
}
