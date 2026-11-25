using System;
using Stilargo.Core.Hill;
using Stilargo.Core.Hud;
using Stilargo.Core.Match;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>Feeds the hill's remaining hold time to FL's HUD; the timer hides once the match is decided.</summary>
    public sealed class HillStatus : IHillStatus
    {
        private readonly HillControl _hill;
        private readonly MatchReferee _referee;

        public HillStatus(HillControl hill, MatchReferee referee)
        {
            _hill = hill ?? throw new ArgumentNullException(nameof(hill));
            _referee = referee ?? throw new ArgumentNullException(nameof(referee));
        }

        public bool IsActive => !_referee.IsDecided;

        public float SecondsRemaining => _hill.RemainingHoldSeconds;
    }
}
