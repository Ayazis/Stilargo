using System.Collections.Generic;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Turns Core roof geometry into Unity meshes.</summary>
    public static class RoofMeshes
    {
        public static Mesh Create(RoofGeometry geometry)
        {
            var vertices = new List<Vector3>(geometry.Vertices.Count);
            foreach (System.Numerics.Vector3 vertex in geometry.Vertices)
            {
                vertices.Add(new Vector3(vertex.X, vertex.Y, vertex.Z));
            }

            var uvs = new List<Vector2>(geometry.Uvs.Count);
            foreach (System.Numerics.Vector2 uv in geometry.Uvs)
            {
                uvs.Add(new Vector2(uv.X, uv.Y));
            }

            var triangles = new List<int>(geometry.Triangles);
            var mesh = new Mesh { name = "Roof" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
