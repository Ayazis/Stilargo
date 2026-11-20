namespace Stilargo.Core.Combat.Stances
{
    /// <summary>Never moves; shoots only what is already in range.</summary>
    public sealed class HoldStance : IStance
    {
        public string Name => "Hold";

        public StanceAction Decide(StanceContext context)
        {
            if (context.Target.Exists && context.Target.IsInWeaponRange)
            {
                return StanceAction.Fire;
            }

            return StanceAction.Idle;
        }
    }
}
