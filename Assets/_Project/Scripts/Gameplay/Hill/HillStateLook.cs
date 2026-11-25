using System;
using Stilargo.Core.Hill;
using UnityEngine;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>The marker material shown for one hill state; set by the scene builder.</summary>
    [Serializable]
    public sealed class HillStateLook
    {
        [SerializeField] private HillState _state;
        [SerializeField] private Material _material;

        public HillState State => _state;

        public Material Material => _material;
    }
}
