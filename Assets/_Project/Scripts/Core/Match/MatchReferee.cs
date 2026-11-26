using System;
using System.Collections.Generic;
using System.Linq;
using Stilargo.Core.Combat;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;

namespace Stilargo.Core.Match
{
    /// <summary>
    /// Decides the phase-1 match: the player wins by holding the hill for its full hold duration and loses once
    /// they own no house and have no unit left alive. The first result is final.
    /// </summary>
    public sealed class MatchReferee
    {
        private readonly HouseClaim _playerHouses;

        public MatchReferee(HillControl hill, HouseClaim playerHouses)
        {
            if (hill == null)
            {
                throw new ArgumentNullException(nameof(hill));
            }

            _playerHouses = playerHouses ?? throw new ArgumentNullException(nameof(playerHouses));
            hill.HoldCompleted += () => Decide(MatchOutcome.Won);
        }

        /// <summary>Raised once, with <see cref="MatchOutcome.Won"/> or <see cref="MatchOutcome.Lost"/>.</summary>
        public event Action<MatchOutcome> Decided;

        public MatchOutcome Outcome { get; private set; } = MatchOutcome.Undecided;

        public bool IsDecided => Outcome != MatchOutcome.Undecided;

        /// <summary>Checks the lose condition against the player's units; call once per frame.</summary>
        public void Review(IReadOnlyList<ICombatTarget> playerUnits)
        {
            if (playerUnits == null)
            {
                throw new ArgumentNullException(nameof(playerUnits));
            }

            if (!_playerHouses.IsClaimed && !playerUnits.Any(unit => unit.IsAlive))
            {
                Decide(MatchOutcome.Lost);
            }
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
