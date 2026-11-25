namespace Stilargo.Core.Hill
{
    /// <summary>Who stands on the hill. Only <see cref="HeldByPlayer"/> earns income and counts toward the win.</summary>
    public enum HillState
    {
        Empty,
        HeldByPlayer,
        HeldByRival,
        Contested,
    }
}
