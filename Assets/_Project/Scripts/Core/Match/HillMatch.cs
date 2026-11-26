using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Hill;
using Stilargo.Core.Rival;

namespace Stilargo.Core.Match
{
    /// <summary>One frame of the phase-1 match: the hill, the referee, then the rival. Both stop once the referee has decided.</summary>
    public sealed class HillMatch
    {
        private readonly RivalBrain _rival;

        public HillMatch(HillControl hill, RivalBrain rival, MatchReferee referee)
        {
            Hill = hill ?? throw new ArgumentNullException(nameof(hill));
            _rival = rival ?? throw new ArgumentNullException(nameof(rival));
            Referee = referee ?? throw new ArgumentNullException(nameof(referee));
        }

        public HillControl Hill { get; }

        public MatchReferee Referee { get; }

        public void Tick(float deltaSeconds, IReadOnlyList<ICombatTarget> playerUnits, IReadOnlyList<ICombatTarget> rivalUnits)
        {
            if (Referee.IsDecided)
            {
                return;
            }

            Hill.Tick(deltaSeconds, playerUnits, rivalUnits);
            Referee.Review(playerUnits);
            if (Referee.IsDecided)
            {
                return;
            }

            _rival.Tick(deltaSeconds);
        }
    }
}
