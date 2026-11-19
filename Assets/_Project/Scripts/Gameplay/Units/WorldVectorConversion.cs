namespace Stilargo.Gameplay.Units
{
    /// <summary>
    /// Converts 3D world positions between Core's System.Numerics vectors and Unity vectors.
    /// </summary>
    public static class WorldVectorConversion
    {
        public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.x, vector.y, vector.z);
        }

        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 vector)
        {
            return new UnityEngine.Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
