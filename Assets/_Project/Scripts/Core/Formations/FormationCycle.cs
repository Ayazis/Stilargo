using System;
using System.Collections.Generic;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// The formations the player can switch between, in order. Starts at the first and wraps around.
    /// </summary>
    public sealed class FormationCycle
    {
        private readonly IFormation[] _formations;
        private int _currentIndex;

        public FormationCycle(IReadOnlyList<IFormation> formations)
        {
            if (formations == null)
            {
                throw new ArgumentNullException(nameof(formations));
            }

            if (formations.Count == 0)
            {
                throw new ArgumentException("Needs at least one formation.", nameof(formations));
            }

            _formations = new IFormation[formations.Count];
            for (int i = 0; i < formations.Count; i++)
            {
                _formations[i] = formations[i] ?? throw new ArgumentException($"Formation {i} is null.", nameof(formations));
            }
        }

        public IFormation Current => _formations[_currentIndex];

        /// <summary>Moves to the next formation, wrapping after the last.</summary>
        /// <returns>The new current formation.</returns>
        public IFormation Next()
        {
            _currentIndex = (_currentIndex + 1) % _formations.Length;
            return Current;
        }
    }
}
