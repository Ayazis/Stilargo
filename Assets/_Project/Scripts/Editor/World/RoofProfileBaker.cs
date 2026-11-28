using System;
using Stilargo.Gameplay.City;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Rasterises a building prefab's meshes from above into a <see cref="RoofProfile"/>: for every cell the highest
    /// surface over its centre, in the prefab's own frame.
    /// </summary>
    public static class RoofProfileBaker
    {
        private const float CellSizeMetres = 0.25f;

        /// <summary>Bakes the profile into the prefab asset and returns the prefab.</summary>
        public static GameObject BakeInto(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                RoofProfile profile = root.GetComponent<RoofProfile>();
                if (profile == null)
                {
                    profile = root.AddComponent<RoofProfile>();
                }

                profile.Bake(Bake(root));
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static RoofProfileData Bake(GameObject root)
        {
            MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();
            if (filters.Length == 0)
            {
                throw new InvalidOperationException($"Building prefab '{root.name}' has no meshes to bake a roof from.");
            }

            Bounds bounds = LocalBounds(root.transform, filters);
            int columns = Mathf.CeilToInt(bounds.size.x / CellSizeMetres);
            int rows = Mathf.CeilToInt(bounds.size.z / CellSizeMetres);
            var raster = new RoofRaster(bounds.min.x, bounds.min.z, CellSizeMetres, columns, rows);
            foreach (MeshFilter filter in filters)
            {
                RasteriseMesh(root.transform, filter, raster);
            }

            return new RoofProfileData(bounds.min.x, bounds.min.z, CellSizeMetres, columns, raster.Heights);
        }

        private static Bounds LocalBounds(Transform root, MeshFilter[] filters)
        {
            var bounds = new Bounds(root.InverseTransformPoint(filters[0].transform.position), Vector3.zero);
            foreach (MeshFilter filter in filters)
            {
                foreach (Vector3 vertex in filter.sharedMesh.vertices)
                {
                    bounds.Encapsulate(root.InverseTransformPoint(filter.transform.TransformPoint(vertex)));
                }
            }

            return bounds;
        }

        private static void RasteriseMesh(Transform root, MeshFilter filter, RoofRaster raster)
        {
            Vector3[] vertices = filter.sharedMesh.vertices;
            for (int index = 0; index < vertices.Length; index++)
            {
                vertices[index] = root.InverseTransformPoint(filter.transform.TransformPoint(vertices[index]));
            }

            int[] triangles = filter.sharedMesh.triangles;
            for (int index = 0; index < triangles.Length; index += 3)
            {
                raster.Raise(vertices[triangles[index]], vertices[triangles[index + 1]], vertices[triangles[index + 2]]);
            }
        }
    }
}
