using System.Numerics;

namespace Stilargo.Core.Camera
{
    /// <summary>
    /// Where the camera looks on the ground and how high above it sits.
    /// </summary>
    public readonly struct CameraState
    {
        public CameraState(Vector2 focus, float height)
            : this(focus, height, 0f)
        {
        }

        /// <param name="yawDegrees">Which way the view faces; 0 is world +Z, growing clockwise seen from above.</param>
        public CameraState(Vector2 focus, float height, float yawDegrees)
        {
            Focus = focus;
            Height = height;
            YawDegrees = yawDegrees;
        }

        /// <summary>The ground point the camera looks at. X is world X, Y is world Z.</summary>
        public Vector2 Focus { get; }

        public float Height { get; }

        /// <summary>Pan input is turned by this so "forward" always means up the screen.</summary>
        public float YawDegrees { get; }
    }
}
