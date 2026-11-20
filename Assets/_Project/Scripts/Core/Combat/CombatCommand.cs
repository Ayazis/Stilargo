using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>The result of a combat tick for Gameplay to carry out.</summary>
    public readonly struct CombatCommand
    {
        private CombatCommand(CombatCommandAction action, Vector3 destination)
        {
            Action = action;
            Destination = destination;
        }

        public static CombatCommand Idle => new CombatCommand(CombatCommandAction.Idle, Vector3.Zero);

        public static CombatCommand Fire => new CombatCommand(CombatCommandAction.Fire, Vector3.Zero);

        public static CombatCommand FollowOrders => new CombatCommand(CombatCommandAction.FollowOrders, Vector3.Zero);

        public CombatCommandAction Action { get; }

        /// <summary>Only meaningful for <see cref="CombatCommandAction.MoveTo"/>.</summary>
        public Vector3 Destination { get; }

        public static CombatCommand MoveTo(Vector3 destination)
        {
            return new CombatCommand(CombatCommandAction.MoveTo, destination);
        }
    }
}
