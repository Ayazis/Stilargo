using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>Whether a world position can currently be seen by this unit's side (fog of war).</summary>
    public interface IVisibilityQuery
    {
        bool IsVisible(Vector3 position);
    }
}
