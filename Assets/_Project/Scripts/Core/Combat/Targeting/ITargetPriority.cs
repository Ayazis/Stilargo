using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Combat.Targeting
{
    /// <summary>
    /// Decides which enemy a unit engages first.
    /// </summary>
    public interface ITargetPriority
    {
        /// <summary>
        /// The preferred living candidate, or null when there is none. Dead candidates are ignored.
        /// </summary>
        ICombatTarget Choose(Vector3 shooterPosition, IReadOnlyList<ICombatTarget> candidates);
    }
}
