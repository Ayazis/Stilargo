using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.City
{
    /// <summary>A roof mesh in local space: base centred on the origin at y = 0, UVs in metres.</summary>
    public sealed class RoofGeometry
    {
        public RoofGeometry(IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector2> uvs, IReadOnlyList<int> triangles)
        {
            Vertices = vertices;
            Uvs = uvs;
            Triangles = triangles;
        }

        public IReadOnlyList<Vector3> Vertices { get; }

        public IReadOnlyList<Vector2> Uvs { get; }

        public IReadOnlyList<int> Triangles { get; }
    }
}
