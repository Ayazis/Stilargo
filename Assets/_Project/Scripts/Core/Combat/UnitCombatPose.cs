using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>Where a unit is and where it was last told to be.</summary>
    public readonly struct UnitCombatPose
    {
        public UnitCombatPose(Vector3 position, Vector3 anchor)
        {
            Position = position;
            Anchor = anchor;
        }

        public Vector3 Position { get; }

        public Vector3 Anchor { get; }
    }
}
