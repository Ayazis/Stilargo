namespace Stilargo.Core.Combat
{
    /// <summary>Random numbers for combat rolls, behind an interface so tests stay deterministic.</summary>
    public interface IRandomSource
    {
        /// <summary>A value in [0, 1).</summary>
        float NextFloat();
    }
}
