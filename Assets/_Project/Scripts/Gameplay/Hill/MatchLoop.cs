using System;
using System.Collections.Generic;
using Stilargo.Core.Combat;
using Stilargo.Core.Match;
using UnityEngine;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>Ticks Core's <see cref="HillMatch"/> every frame with the live unit lists of both sides.</summary>
    public sealed class MatchLoop : MonoBehaviour
    {
        private HillMatch _match;
        private IReadOnlyList<ICombatTarget> _playerUnits;
        private IReadOnlyList<ICombatTarget> _rivalUnits;

        public void Construct(HillMatch match, IReadOnlyList<ICombatTarget> playerUnits, IReadOnlyList<ICombatTarget> rivalUnits)
        {
            if (_match != null)
            {
                throw new InvalidOperationException($"{nameof(MatchLoop)} is already constructed.");
            }

            _match = match ?? throw new ArgumentNullException(nameof(match));
            _playerUnits = playerUnits ?? throw new ArgumentNullException(nameof(playerUnits));
            _rivalUnits = rivalUnits ?? throw new ArgumentNullException(nameof(rivalUnits));
        }

        private void Update()
        {
            if (_match == null)
            {
                return;
            }

            _match.Tick(Time.deltaTime, _playerUnits, _rivalUnits);        }
    }
}
