using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>The thin slab of lawn, gravel, paving or earth that open ground is surfaced with.</summary>
    internal static class GroundCover
    {
        private const float Half = 0.5f;

        public static GameObject Lay(PropFactory props, PlotFrame frame, float height, Material material)
        {
            var size = new Vector3(frame.Width, height, frame.Depth);
            return PropFactory.IgnoredByNavigation(props.Box(frame.GroundCentre + (Vector3.up * (height * Half)), size, material));
        }
    }
}
