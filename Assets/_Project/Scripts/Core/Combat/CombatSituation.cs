using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>The world as one unit sees it this tick.</summary>
    public readonly struct CombatSituation
    {
        public CombatSituation(UnitCombatPose pose, bool isFollowingMoveOrder, IReadOnlyList<ICombatTarget> enemies)
        {
            Pose = pose;
            IsFollowingMoveOrder = isFollowingMoveOrder;
            Enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
        }

        public UnitCombatPose Pose { get; }

        public Vector3 UnitPosition => Pose.Position;

        public Vector3 Anchor => Pose.Anchor;

        public bool IsFollowingMoveOrder { get; }

        /// <summary>Enemy candidates; dead or unseen ones are filtered out by the unit.</summary>
        public IReadOnlyList<ICombatTarget> Enemies { get; }
    }
}
