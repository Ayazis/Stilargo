using System;

namespace Stilargo.Core.Combat.Stances
{
    /// <summary>
    /// Guards its anchor: chases only targets inside the leash radius around the anchor, otherwise walks back.
    /// </summary>
    public sealed class DefensiveStance : IStance
    {
        /// <summary>Close enough to the anchor to count as home, so units don't jitter around it.</summary>
        public const float AnchorArrivalTolerance = 0.5f;

        private readonly float _leashRadius;

        public DefensiveStance(float leashRadius)
        {
            if (!(leashRadius > 0f) || float.IsInfinity(leashRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(leashRadius), leashRadius, "Leash radius must be a positive finite number of metres.");
            }

            _leashRadius = leashRadius;
        }

        public string Name => "Defensive";

        public float LeashRadius => _leashRadius;

        public StanceAction Decide(StanceContext context)
        {
            StanceTarget target = context.Target;
            if (target.Exists && target.IsInWeaponRange)
            {
                return StanceAction.Fire;
            }

            if (target.Exists && IsWithinLeash(context, target))
            {
                return StanceAction.Chase;
            }

            return IsAwayFromAnchor(context) ? StanceAction.ReturnToAnchor : StanceAction.Idle;
        }

        private bool IsWithinLeash(StanceContext context, StanceTarget target)
        {
            return GroundDistance.Between(context.Anchor, target.Position) <= _leashRadius;
        }

        private static bool IsAwayFromAnchor(StanceContext context)
        {
            return GroundDistance.Between(context.UnitPosition, context.Anchor) > AnchorArrivalTolerance;
        }
    }
}
