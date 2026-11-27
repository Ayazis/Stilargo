using UnityEngine;

namespace Stilargo.Editor.Scenes
{
    /// <summary>What a unit is built from: its avatar prefab and marker materials, grouped so unit creation stays within three parameters.</summary>
    internal readonly struct UnitLook
    {
        public UnitLook(GameObject avatar, Material marker, Material path)
        {
            Avatar = avatar;
            Marker = marker;
            Path = path;
        }

        public GameObject Avatar { get; }

        public Material Marker { get; }

        public Material Path { get; }
    }
}
