using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The materials the city's landmarks are dressed in. Placeholders made by <c>Stilargo → Build World Prefab</c>;
    /// swap any of them for an authored PBR material on the World prefab.
    /// </summary>
    [Serializable]
    public sealed class LandmarkMaterials
    {
        [SerializeField] private Material _stone;
        [SerializeField] private Material _schoolWall;
        [SerializeField] private Material _tavernWall;
        [SerializeField] private Material _grass;
        [SerializeField] private Material _foliage;
        [SerializeField] private Material _bark;
        [SerializeField] private Material _water;
        [SerializeField] private Material _wood;
        [SerializeField] private Material _cloth;
        [SerializeField] private Material _shadow;

        /// <summary>Church walls, tower, pond rims and kerbs.</summary>
        public Material Stone => _stone;

        public Material SchoolWall => _schoolWall;

        public Material TavernWall => _tavernWall;

        public Material Grass => _grass;

        public Material Foliage => _foliage;

        public Material Bark => _bark;

        public Material Water => _water;

        /// <summary>Signs, benches and barrels.</summary>
        public Material Wood => _wood;

        /// <summary>Flags and awnings.</summary>
        public Material Cloth => _cloth;

        /// <summary>The dark inside of openings such as the belfry.</summary>
        public Material Shadow => _shadow;

        public bool IsComplete => _stone != null && _schoolWall != null && _tavernWall != null && _grass != null
            && _foliage != null && _bark != null && _water != null && _wood != null && _cloth != null && _shadow != null;
    }
}
