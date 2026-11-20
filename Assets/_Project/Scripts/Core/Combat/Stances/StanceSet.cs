using System;

namespace Stilargo.Core.Combat.Stances
{
    /// <summary>The three stances the player can pick from.</summary>
    public sealed class StanceSet
    {
        public StanceSet(IStance aggressive, IStance defensive, IStance hold)
        {
            Aggressive = aggressive ?? throw new ArgumentNullException(nameof(aggressive));
            Defensive = defensive ?? throw new ArgumentNullException(nameof(defensive));
            Hold = hold ?? throw new ArgumentNullException(nameof(hold));
        }

        public IStance Aggressive { get; }

        public IStance Defensive { get; }

        public IStance Hold { get; }
    }
}
