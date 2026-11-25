using System;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;

namespace Stilargo.Core.Match
{
    /// <summary>
    /// Decides the phase-1 match: the player wins by holding the hill for its full hold duration and loses when
    /// the base (the first claimed house) is destroyed. The first result is final.
    /// </summary>
    public sealed class MatchReferee
    {
        public MatchReferee(HillControl hill, HouseClaim claim)
        {
            if (hill == null)
            {
                throw new ArgumentNullException(nameof(hill));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            hill.HoldCompleted += () => Decide(MatchOutcome.Won);
            claim.Claimed += house => WatchIfBase(claim, house);
            if (claim.IsClaimed)
            {
                WatchHouse(claim.House);
            }
        }

        /// <summary>Raised once, with <see cref="MatchOutcome.Won"/> or <see cref="MatchOutcome.Lost"/>.</summary>
        public event Action<MatchOutcome> Decided;

        public MatchOutcome Outcome { get; private set; } = MatchOutcome.Undecided;

        public bool IsDecided => Outcome != MatchOutcome.Undecided;

        // Only the base decides the match; other claimed houses can fall without losing it.
        private void WatchIfBase(HouseClaim claim, ClaimedHouse house)
        {
            if (house == claim.House)
            {
                WatchHouse(house);
            }
        }

        private void WatchHouse(ClaimedHouse house)
        {
            house.Died += () => Decide(MatchOutcome.Lost);
        }

        private void Decide(MatchOutcome outcome)
        {
            if (IsDecided)
            {
                return;
            }

            Outcome = outcome;
            Decided?.Invoke(outcome);
        }
    }
}
