using System;
using System.Linq;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Cover;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>Places a greybox block on every slot of the claimed house that gets a fortification.</summary>
    public sealed class FortificationView : MonoBehaviour
    {
        private const string BlockNameFormat = "Fortification ({0})";

        [SerializeField] private FortificationLook[] _looks = Array.Empty<FortificationLook>();

        private HouseClaim _claim;

        public void Construct(HouseClaim claim)
        {
            if (_claim != null)
            {
                throw new InvalidOperationException($"{nameof(FortificationView)} is already constructed.");
            }

            _claim = claim ?? throw new ArgumentNullException(nameof(claim));
            RequireLookForEveryKind();
            _claim.Claimed += HandleClaimed;
        }

        private void OnDestroy()
        {
            if (_claim == null)
            {
                return;
            }

            _claim.Claimed -= HandleClaimed;
            foreach (ClaimedHouse house in _claim.Houses)
            {
                house.FortificationBuilt -= HandleBuilt;
            }
        }

        private void HandleClaimed(ClaimedHouse house)
        {
            house.FortificationBuilt += HandleBuilt;
        }

        private void HandleBuilt(FortificationSlot slot)
        {
            FortificationLook look = _looks.First(candidate => candidate.SlotKind == slot.Kind);
            GameObject block = new GameObject(string.Format(BlockNameFormat, slot.Fortification.DisplayName));
            block.transform.SetParent(transform, false);

            // Blocks rest on the slot: lift by half their height so they don't sink into the floor or roof.
            block.transform.position = slot.Position.ToUnity() + (Vector3.up * (look.SizeMetres.y / 2f));
            block.transform.rotation = Quaternion.LookRotation(slot.Facing.ToUnity(), Vector3.up);
            block.transform.localScale = look.SizeMetres;
            block.AddComponent<MeshFilter>().sharedMesh = PrimitiveMeshes.Cube;
            block.AddComponent<MeshRenderer>().sharedMaterial = look.Material;
        }

        private void RequireLookForEveryKind()
        {
            foreach (FortificationSlotKind kind in Enum.GetValues(typeof(FortificationSlotKind)))
            {
                FortificationLook look = _looks.FirstOrDefault(candidate => candidate != null && candidate.SlotKind == kind);
                if (look == null || look.Material == null)
                {
                    throw new InvalidOperationException($"{nameof(FortificationView)} on '{name}' has no look with a material for {kind} slots.");
                }
            }
        }
    }
}
