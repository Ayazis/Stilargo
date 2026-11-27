using System;
using Stilargo.Core.Rival;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The <c>rival</c> section of <c>Hill.json</c>. Defaults match a new <see cref="RivalSettings"/>.</summary>
    [Serializable]
    public sealed class RivalEntry
    {
        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private int squadSize = RivalSettings.DefaultSquadSize;
        [SerializeField] private float moveSpeedMetresPerSecond = RivalSettings.DefaultMoveSpeedMetresPerSecond;
        [SerializeField] private float scavengeSeconds = RivalSettings.DefaultScavengeSeconds;
        [SerializeField] private float contestSeconds = RivalSettings.DefaultContestSeconds;
        [SerializeField] private float raidSeconds = RivalSettings.DefaultRaidSeconds;
        [SerializeField] private float confiscateSeconds = RivalSettings.DefaultConfiscateSeconds;
        [SerializeField] private float scavengeReachMetres = RivalSettings.DefaultScavengeReachMetres;
        [SerializeField] private float scavengeRadiusMetres = RivalSettings.DefaultScavengeRadiusMetres;
        [SerializeField] private float hillSpreadMetres = RivalSettings.DefaultHillSpreadMetres;
        [SerializeField] private float arrivalToleranceMetres = RivalSettings.DefaultArrivalToleranceMetres;

        public RivalSettings ToSettings()
        {
            try
            {
                return new RivalSettings()
                    .WithSquadSize(squadSize)
                    .WithMoveSpeed(moveSpeedMetresPerSecond)
                    .WithStrategyTimes(scavengeSeconds, contestSeconds, raidSeconds)
                    .WithConfiscateTime(confiscateSeconds)
                    .WithScavengeReach(scavengeReachMetres)
                    .WithScavengeRadius(scavengeRadiusMetres)
                    .WithHillSpread(hillSpreadMetres)
                    .WithArrivalTolerance(arrivalToleranceMetres);
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"rival: {exception.Message}", exception);
            }
        }
    }
}
