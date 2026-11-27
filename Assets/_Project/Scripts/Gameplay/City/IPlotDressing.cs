using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>What makes one plot use look like itself: the wall it is built in and anything added around it.</summary>
    internal interface IPlotDressing
    {
        /// <summary>False keeps the house wall palette.</summary>
        bool TryGetWallMaterial(out Material material);

        void Dress(PlotFrame frame, PropFactory props);
    }
}
