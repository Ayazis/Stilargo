using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>
    /// Something a unit can aim at. Positions are world space, Y up.
    /// </summary>
    public interface ICombatTarget : IDamageable
    {
        Vector3 Position { get; }

        /// <summary>Remaining health as a fraction in [0, 1].</summary>
        float HealthFraction { get; }
    }
}
