using System.Numerics;

namespace Stilargo.Core.Selection
{
    /// <summary>
    /// Projects world positions to screen pixels (origin bottom-left). Implemented outside Core by the camera.
    /// </summary>
    public interface IScreenProjector
    {
        /// <returns>False when the point is behind the camera and has no meaningful screen position.</returns>
        bool TryProject(Vector3 world, out Vector2 screen);
    }
}
