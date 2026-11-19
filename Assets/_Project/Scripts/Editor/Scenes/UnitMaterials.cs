using UnityEngine;

namespace Stilargo.Editor.Scenes
{
    /// <summary>The materials a greybox unit is built from, grouped so unit creation stays within three parameters.</summary>
    internal readonly struct UnitMaterials
    {
        public UnitMaterials(Material body, Material marker, Material path)
        {
            Body = body;
            Marker = marker;
            Path = path;
        }

        public Material Body { get; }

        public Material Marker { get; }

        public Material Path { get; }
    }
}
