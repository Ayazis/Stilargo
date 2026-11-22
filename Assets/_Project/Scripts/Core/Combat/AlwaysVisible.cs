using System.Numerics;

namespace Stilargo.Core.Combat
{
    /// <summary>Visibility without fog of war: every position is visible (used for the AI side).</summary>
    public sealed class AlwaysVisible : IVisibilityQuery
    {
        public bool IsVisible(Vector3 position)
        {
            return true;
        }
    }
}
