namespace Stilargo.Gameplay.CameraControl
{
    /// <summary>
    /// Converts between Core's System.Numerics vectors and Unity vectors.
    /// Core's 2D ground vectors use Y for world Z.
    /// </summary>
    public static class VectorConversion
    {
        public static System.Numerics.Vector2 ToNumerics(this UnityEngine.Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.x, vector.y);
        }

        /// <summary>Drops the height and keeps the ground position (world X, world Z).</summary>
        public static System.Numerics.Vector2 ToGroundPlane(this UnityEngine.Vector3 position)
        {
            return new System.Numerics.Vector2(position.x, position.z);
        }

        public static UnityEngine.Vector3 ToWorldPosition(this System.Numerics.Vector2 groundPoint, float height)
        {
            return new UnityEngine.Vector3(groundPoint.X, height, groundPoint.Y);
        }
    }
}
