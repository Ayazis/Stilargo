using System;
using Stilargo.Core.Match;
using UnityEngine;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>Freezes the game once the match is won or lost, so the result stays on screen.</summary>
    public sealed class MatchEndPause : MonoBehaviour
    {
        private const float PausedTimeScale = 0f;
        private const float NormalTimeScale = 1f;

        private MatchReferee _referee;
        private bool _isSubscribed;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            // Time scale outlives the scene, so a frozen match must not freeze whatever loads next.
            Time.timeScale = NormalTimeScale;
        }

        public void Construct(MatchReferee referee)
        {
            if (_referee != null)
            {
                throw new InvalidOperationException($"{nameof(MatchEndPause)} is already constructed.");
            }

            _referee = referee ?? throw new ArgumentNullException(nameof(referee));
            Subscribe();
        }

        private void Pause(MatchOutcome outcome)
        {
            Time.timeScale = PausedTimeScale;
        }

        private void Subscribe()
        {
            if (_referee == null || _isSubscribed)
            {
                return;
            }

            _referee.Decided += Pause;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _referee.Decided -= Pause;
            _isSubscribed = false;
        }
    }
}
