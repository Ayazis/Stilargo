namespace Stilargo.Core.Combat.Stances
{
    /// <summary>What a stance wants the unit to do this tick.</summary>
    public enum StanceAction
    {
        /// <summary>Stay where it is and don't shoot.</summary>
        Idle,

        /// <summary>Stand still and shoot the target.</summary>
        Fire,

        /// <summary>Move towards the target until it is in range.</summary>
        Chase,

        /// <summary>Walk back to the anchor.</summary>
        ReturnToAnchor
    }
}
