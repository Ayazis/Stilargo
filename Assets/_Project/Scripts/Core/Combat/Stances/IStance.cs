namespace Stilargo.Core.Combat.Stances
{
    /// <summary>
    /// How eagerly an idle unit engages enemies. Only consulted while the unit has no move order.
    /// </summary>
    public interface IStance
    {
        /// <summary>Shown to the player, for example "Aggressive".</summary>
        string Name { get; }

        StanceAction Decide(StanceContext context);
    }
}
