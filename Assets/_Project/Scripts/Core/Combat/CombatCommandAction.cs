namespace Stilargo.Core.Combat
{
    /// <summary>What Gameplay should do with the unit after a combat tick.</summary>
    public enum CombatCommandAction
    {
        /// <summary>Stand still.</summary>
        Idle,

        /// <summary>Stand still; the unit is engaging its target.</summary>
        Fire,

        /// <summary>Walk to the command's destination.</summary>
        MoveTo,

        /// <summary>Keep executing the current move order; combat does not steer.</summary>
        FollowOrders
    }
}
