using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The materials the city's open ground is surfaced with. Placeholders made by <c>Stilargo → Build World Prefab</c>;
    /// swap any of them for an authored PBR material on the World prefab.
    /// </summary>
    [Serializable]
    public sealed class LandmarkMaterials
    {
        [SerializeField] private Material _stone;
        [SerializeField] private Material _grass;
        [SerializeField] private Material _water;
        [SerializeField] private Material _wood;

        /// <summary>Paving, gravel, kerbs and the canal bed.</summary>
        public Material Stone => _stone;

        public Material Grass => _grass;

        public Material Water => _water;

        /// <summary>The bare earth of fields.</summary>
        public Material Wood => _wood;

        public bool IsComplete => _stone != null && _grass != null && _water != null && _wood != null;
    }
}
