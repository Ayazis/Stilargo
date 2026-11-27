namespace Stilargo.Core.Rival
{
    /// <summary>The rival's strategies, one instance each, so they can name each other as transitions.</summary>
    public sealed class RivalStrategySet
    {
        public IRivalStrategy Scavenge { get; } = new ScavengeStrategy();

        public IRivalStrategy ContestHill { get; } = new ContestHillStrategy();

        public IRivalStrategy RaidHouse { get; } = new RaidHouseStrategy();

        public IRivalStrategy ConfiscateHouse { get; } = new ConfiscateHouseStrategy();

        /// <summary>The rival opens by scavenging, which gives the player time to explore.</summary>
        public IRivalStrategy Initial => Scavenge;
    }
}
