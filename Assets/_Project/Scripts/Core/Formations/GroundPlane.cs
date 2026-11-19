using System.Numerics;

namespace Stilargo.Core.Formations
{
    /// <summary>
    /// Directions and distances on the X/Z ground plane (Y up), shared by formations and slot assignment.
    /// </summary>
    public static class GroundPlane
    {
        /// <summary>The forward used when a facing has no ground-plane direction (e.g. a zero vector or straight up).</summary>
        public static readonly Vector3 DefaultForward = Vector3.UnitZ;

        // Below this squared length a facing is treated as having no direction, to avoid normalising noise.
        private const float MinDirectionLengthSquared = 1e-8f;

        /// <summary>The unit-length ground-plane direction of <paramref name="facing"/>, or <see cref="DefaultForward"/>.</summary>
        public static Vector3 ForwardOf(Vector3 facing)
        {
            var flat = new Vector3(facing.X, 0f, facing.Z);

            if (!(flat.LengthSquared() > MinDirectionLengthSquared))
            {
                return DefaultForward;
            }

            return Vector3.Normalize(flat);
        }

        /// <summary>The ground-plane direction to the right of <paramref name="forward"/>, seen from above with Y up.</summary>
        public static Vector3 RightOf(Vector3 forward)
        {
            return new Vector3(forward.Z, 0f, -forward.X);
        }

        /// <summary>Squared distance ignoring height, so slopes do not skew which slot is closest.</summary>
        public static float DistanceSquared(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return (dx * dx) + (dz * dz);
        }
    }
}
