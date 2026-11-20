namespace Stilargo.Core.Combat.Stances
{
    /// <summary>Shoots anything in range and chases anything it can see.</summary>
    public sealed class AggressiveStance : IStance
    {
        public string Name => "Aggressive";

        public StanceAction Decide(StanceContext context)
        {
            if (!context.Target.Exists)
            {
                return StanceAction.Idle;
            }

            return context.Target.IsInWeaponRange ? StanceAction.Fire : StanceAction.Chase;
        }
    }
}
