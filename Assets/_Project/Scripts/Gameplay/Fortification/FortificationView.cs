using System;
using System.Linq;
using Stilargo.Core.Fortification;
using Stilargo.Gameplay.Cover;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Fortification
{
    /// <summary>
    /// Places a greybox block on every slot of a claimed house that gets a fortification. Turrets are left out:
    /// <see cref="TurretBattery"/> places their models, because it also aims them.
    /// </summary>
    public sealed class FortificationView : MonoBehaviour
    {
        private const string BlockNameFormat = "Fortification ({0})";

        [SerializeField] private FortificationLook[] _looks = Array.Empty<FortificationLook>();

        private HouseLedger _ledger;

        public void Construct(HouseLedger ledger)
        {
            if (_ledger != null)
            {
                throw new InvalidOperationException($"{nameof(FortificationView)} is already constructed.");
            }

            _ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            RequireLooksHaveMaterials();
            _ledger.HouseCreated += HandleHouseCreated;
        }

        private void OnDestroy()
        {
            if (_ledger == null)
            {
                return;
            }

            _ledger.HouseCreated -= HandleHouseCreated;
            foreach (ClaimedHouse house in _ledger.Houses)
            {
                house.FortificationBuilt -= HandleBuilt;
            }
        }

        private void HandleHouseCreated(ClaimedHouse house)
        {
            house.FortificationBuilt += HandleBuilt;
        }

        private void HandleBuilt(FortificationSlot slot)
        {
            if (slot.Fortification.IsTurret)
            {
                return;
            }

            FortificationLook look = _looks.FirstOrDefault(candidate => candidate.SlotKind == slot.Kind);
            if (look == null)
            {
                throw new InvalidOperationException($"{nameof(FortificationView)} on '{name}' has no look for {slot.Kind} slots.");
            }

            GameObject block = new GameObject(string.Format(BlockNameFormat, slot.Fortification.DisplayName));
            block.transform.SetParent(transform, false);

            // Blocks rest on the slot: lift by half their height so they don't sink into the floor or roof.
            block.transform.position = slot.Position.ToUnity() + (Vector3.up * (look.SizeMetres.y / 2f));
            block.transform.rotation = Quaternion.LookRotation(slot.Facing.ToUnity(), Vector3.up);
            block.transform.localScale = look.SizeMetres;
            block.AddComponent<MeshFilter>().sharedMesh = PrimitiveMeshes.Cube;
            block.AddComponent<MeshRenderer>().sharedMaterial = look.Material;
        }

        private void RequireLooksHaveMaterials()
        {
            FortificationLook missing = _looks.FirstOrDefault(look => look == null || look.Material == null);
            if (_looks.Length == 0 || missing != null)
            {
                throw new InvalidOperationException($"{nameof(FortificationView)} on '{name}' needs looks, each with a material.");
            }
        }
    }
}
