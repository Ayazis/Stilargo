using Stilargo.Core.Fog;

namespace Stilargo.Core.City
{
    /// <summary>The top of one roof, so things placed on it (turrets) can stand on it instead of inside it.</summary>
    public interface IRoofSurface
    {
        /// <summary>The roof's height above <paramref name="point"/>; false where the roof does not cover it.</summary>
        bool TryGetHeight(WorldPoint point, out float height);
    }
}
