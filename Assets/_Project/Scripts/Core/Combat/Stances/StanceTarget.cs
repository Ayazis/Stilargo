using System.Numerics;

namespace Stilargo.Core.Combat.Stances
{
    /// <summary>
    /// The enemy the unit's target priority picked, if any, and whether its weapon reaches it.
    /// </summary>
    public readonly struct StanceTarget
    {
        private StanceTarget(bool exists, Vector3 position, bool isInWeaponRange)
        {
            Exists = exists;
            Position = position;
            IsInWeaponRange = isInWeaponRange;
        }

        public static StanceTarget None => new StanceTarget(false, Vector3.Zero, false);

        public bool Exists { get; }

        public Vector3 Position { get; }

        public bool IsInWeaponRange { get; }

        public static StanceTarget At(Vector3 position, bool isInWeaponRange)
        {
            return new StanceTarget(true, position, isInWeaponRange);
        }
    }
}
