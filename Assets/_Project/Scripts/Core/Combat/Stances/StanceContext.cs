using System.Numerics;

namespace Stilargo.Core.Combat.Stances
{
    /// <summary>
    /// What a stance sees when it decides. Positions are world space, Y up; distances are on the ground plane.
    /// </summary>
    public readonly struct StanceContext
    {
        public StanceContext(Vector3 unitPosition, Vector3 anchor, StanceTarget target)
        {
            UnitPosition = unitPosition;
            Anchor = anchor;
            Target = target;
        }

        public Vector3 UnitPosition { get; }

        /// <summary>Where the unit was last told to be: its spawn point or its last move order's end.</summary>
        public Vector3 Anchor { get; }

        public StanceTarget Target { get; }
    }
}
