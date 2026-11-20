using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Combat.Targeting
{
    /// <summary>Engages the closest living enemy on the ground plane.</summary>
    public sealed class NearestTargetPriority : ITargetPriority
    {
        public ICombatTarget Choose(Vector3 shooterPosition, IReadOnlyList<ICombatTarget> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            ICombatTarget best = null;
            float bestDistance = float.MaxValue;

            foreach (ICombatTarget candidate in candidates)
            {
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                float distance = GroundDistance.Between(shooterPosition, candidate.Position);

                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }
}
