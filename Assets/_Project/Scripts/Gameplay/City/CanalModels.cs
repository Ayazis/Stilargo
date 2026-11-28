using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The Blender-built canal pieces (<c>Tools/Art/build_quay.py</c>, <c>build_bridge.py</c>). Each quay segment has
    /// its paving edge at its origin and the water on its +Z side; the bridge spans along Z. Assigned by
    /// <c>Stilargo → Build World Prefab</c>.
    /// </summary>
    [Serializable]
    public sealed class CanalModels
    {
        [SerializeField] private GameObject _quayWall;
        [SerializeField] private GameObject _quaySteps;
        [SerializeField] private GameObject _bridge;
        [SerializeField] private float _segmentLengthMetres = 8f;
        [SerializeField] private float _waterDepthMetres = 1.2f;
        [SerializeField] private float _bedDepthMetres = 2f;
        [SerializeField] private int _segmentsPerSteps = 5;

        public GameObject QuayWall => _quayWall;

        public GameObject QuaySteps => _quaySteps;

        public GameObject Bridge => _bridge;

        /// <summary>Length of one quay segment along the canal; the Blender script builds them 8 m long.</summary>
        public float SegmentLengthMetres => _segmentLengthMetres;

        /// <summary>How far the water lies below the street; must match WATER_LEVEL in build_quay.py.</summary>
        public float WaterDepthMetres => _waterDepthMetres;

        /// <summary>How far the canal bed lies below the street; must match WALL_FOOT in build_quay.py.</summary>
        public float BedDepthMetres => _bedDepthMetres;

        /// <summary>Every this many quay segments, one has steps down to the water.</summary>
        public int SegmentsPerSteps => _segmentsPerSteps;

        public bool IsComplete => _quayWall != null && _quaySteps != null && _bridge != null && _segmentLengthMetres > 0f && _segmentsPerSteps > 0;
    }
}
