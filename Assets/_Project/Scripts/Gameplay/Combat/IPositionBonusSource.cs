using Stilargo.Core.Combat;

namespace Stilargo.Gameplay.Combat
{
    /// <summary>Something that grants a unit a bonus for where it stands right now (cover, a garrison slot).</summary>
    public interface IPositionBonusSource
    {
        PositionBonus CurrentBonus { get; }
    }
}
