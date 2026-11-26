using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>A plain house: palette walls, hip roof and facade kit, nothing more.</summary>
    internal sealed class HouseDressing : IPlotDressing
    {
        public bool TryGetWallMaterial(out Material material)
        {
            material = null;
            return false;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
        }
    }
}
