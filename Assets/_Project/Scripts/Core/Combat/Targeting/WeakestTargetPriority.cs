using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Combat.Targeting
{
    /// <summary>Engages the living enemy with the lowest health fraction; the closer one wins a tie.</summary>
    public sealed class WeakestTargetPriority : ITargetPriority
    {
        public ICombatTarget Choose(Vector3 shooterPosition, IReadOnlyList<ICombatTarget> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            ICombatTarget best = null;
            float bestFraction = float.MaxValue;
            float bestDistance = float.MaxValue;

            foreach (ICombatTarget candidate in candidates)
            {
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                float fraction = candidate.HealthFraction;
                float distance = GroundDistance.Between(shooterPosition, candidate.Position);
                bool isWeaker = fraction < bestFraction;
                bool isCloserTie = fraction == bestFraction && distance < bestDistance;

                if (isWeaker || isCloserTie)
                {
                    best = candidate;
                    bestFraction = fraction;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }
}
