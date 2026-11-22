using System.Collections.Generic;
using UnityEngine;

namespace Stilargo.Gameplay.Cover
{
    /// <summary>Hands out Unity's built-in primitive meshes so many placeholder objects share one mesh each.</summary>
    public static class PrimitiveMeshes
    {
        private static readonly Dictionary<PrimitiveType, Mesh> Cache = new Dictionary<PrimitiveType, Mesh>();

        /// <summary>Unit cube, 1 m on every side.</summary>
        public static Mesh Cube => Get(PrimitiveType.Cube);

        /// <summary>Cylinder 1 m across and 2 m tall (Unity's built-in proportions).</summary>
        public static Mesh Cylinder => Get(PrimitiveType.Cylinder);

        private static Mesh Get(PrimitiveType type)
        {
            if (Cache.TryGetValue(type, out Mesh mesh) && mesh != null)
            {
                return mesh;
            }

            // Only the mesh is wanted; the probe and its collider are thrown away immediately.
            GameObject probe = GameObject.CreatePrimitive(type);
            mesh = probe.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(probe);
            Cache[type] = mesh;
            return mesh;
        }
    }
}
