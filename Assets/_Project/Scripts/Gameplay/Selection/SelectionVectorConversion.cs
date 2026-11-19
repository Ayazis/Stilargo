namespace Stilargo.Gameplay.Selection
{
    /// <summary>
    /// Converts between Core's System.Numerics vectors and Unity vectors for selection and orders.
    /// </summary>
    internal static class SelectionVectorConversion
    {
        public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
        }

        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vector)
        {
            return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
        }

        /// <summary>Screen X/Y of a Unity screen point (pixels, origin bottom-left), dropping depth.</summary>
        public static System.Numerics.Vector2 ToScreenNumerics(this UnityEngine.Vector3 screenPoint)
        {
            return new System.Numerics.Vector2(screenPoint.x, screenPoint.y);
        }

        /// <summary>A screen point (pixels, origin bottom-left) as Unity expects for camera rays.</summary>
        public static UnityEngine.Vector3 ToScreenPoint(this System.Numerics.Vector2 point)
        {
            return new UnityEngine.Vector3(point.X, point.Y, 0f);
        }
    }
}
