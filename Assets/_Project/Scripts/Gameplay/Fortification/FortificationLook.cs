using System;
using Stilargo.Core.Fortification;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>How a built fortification of one slot kind is drawn: a greybox box until real art exists.</summary>
    [Serializable]
    public sealed class FortificationLook
    {
        [SerializeField] private FortificationSlotKind _slotKind;
        [SerializeField] private Material _material;
        [SerializeField] private Vector3 _sizeMetres = Vector3.one;

        public FortificationSlotKind SlotKind => _slotKind;

        public Material Material => _material;

        public Vector3 SizeMetres => _sizeMetres;
    }
}
