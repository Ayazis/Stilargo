using System.Numerics;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Where the camera looks on the ground and how high above it sits.
    /// </summary>
    public readonly struct CameraState
    {
        public CameraState(Vector2 focus, float height)
        {
            Focus = focus;
            Height = height;
        }

        /// <summary>The ground point the camera looks at. X is world X, Y is world Z.</summary>
        public Vector2 Focus { get; }

        public float Height { get; }
    }
}
