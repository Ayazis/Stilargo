namespace Stilargo.Core.Hud
{
    /// <summary>The hill status while the game has no hill, so the HUD hides its timer.</summary>
    public sealed class NoHillStatus : IHillStatus
    {
        public bool IsActive => false;

        public float SecondsRemaining => 0f;
    }
}
