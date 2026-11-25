namespace Stilargo.Core.Hud
{
    /// <summary>
    /// What the HUD needs to know about the hill. Step 10 implements it; until then <see cref="NoHillStatus"/> stands in.
    /// </summary>
    public interface IHillStatus
    {
        /// <summary>True while there is a hill whose hold time the HUD should show.</summary>
        bool IsActive { get; }

        /// <summary>How long the player still has to hold the hill to win.</summary>
        float SecondsRemaining { get; }
    }
}
