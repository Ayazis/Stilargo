namespace Stilargo.Core.Rival
{
    /// <summary>
    /// One state of the rival's state machine. A strategy gives its own orders and decides its own transitions,
    /// so a new behaviour is a new type in <see cref="RivalStrategySet"/>, not another branch in the brain.
    /// </summary>
    public interface IRivalStrategy
    {
        /// <summary>Short display name, e.g. for the HUD.</summary>
        string Name { get; }

        /// <summary>Called when the brain switches to this strategy; resets its timers and gives first orders.</summary>
        void Enter(RivalWorld world);

        /// <summary>Gives follow-up orders and returns the strategy to run next: itself, or one from the set.</summary>
        IRivalStrategy Tick(RivalWorld world, RivalStrategySet strategies, float deltaSeconds);
    }
}
