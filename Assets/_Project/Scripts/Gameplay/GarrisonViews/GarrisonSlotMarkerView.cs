using System;
using System.Collections.Generic;
using Stilargo.Core.Garrison;
using Stilargo.Gameplay.Cover;
using Stilargo.Gameplay.Units;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stilargo.Gameplay.GarrisonViews
{
    /// <summary>Shows a flat plate on every occupied garrison slot; markers are pooled and reused.</summary>
    public sealed class GarrisonSlotMarkerView : MonoBehaviour
    {
        private const float DiameterMetres = 1.4f;
        private const float ThicknessMetres = 0.01f;
        private const float LiftMetres = 0.02f;

        // Unity's built-in cylinder is 2 m tall at scale 1.
        private const float CylinderMeshHeightMetres = 2f;
        private const string MarkerName = "GarrisonSlotMarker";

        [SerializeField] private Material _material;

        private readonly Dictionary<GarrisonSlot, GameObject> _activeMarkers = new Dictionary<GarrisonSlot, GameObject>();
        private readonly Stack<GameObject> _pool = new Stack<GameObject>();
        private IReadOnlyList<GarrisonBuilding> _buildings;
        private bool _isSubscribed;

        public void Construct(IReadOnlyList<GarrisonBuilding> buildings)
        {
            if (buildings == null)
            {
                throw new ArgumentNullException(nameof(buildings));
            }

            if (_material == null)
            {
                throw new InvalidOperationException("GarrisonSlotMarkerView needs a material.");
            }

            if (_buildings != null)
            {
                throw new InvalidOperationException("GarrisonSlotMarkerView is already constructed.");
            }

            _buildings = buildings;
            Subscribe();
            ShowAlreadyOccupied();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // Construct may run before OnEnable (bootstrapper order), so both paths subscribe and the flag stops doubles.
        private void Subscribe()
        {
            if (_isSubscribed || _buildings == null)
            {
                return;
            }

            foreach (GarrisonBuilding building in _buildings)
            {
                building.SlotOccupied += HandleSlotOccupied;
                building.SlotReleased += HandleSlotReleased;
            }

            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            foreach (GarrisonBuilding building in _buildings)
            {
                building.SlotOccupied -= HandleSlotOccupied;
                building.SlotReleased -= HandleSlotReleased;
            }

            _isSubscribed = false;
        }

        private void ShowAlreadyOccupied()
        {
            foreach (GarrisonBuilding building in _buildings)
            {
                foreach (GarrisonSlot slot in building.Slots)
                {
                    if (slot.IsOccupied)
                    {
                        HandleSlotOccupied(slot);
                    }
                }
            }
        }

        private void HandleSlotOccupied(GarrisonSlot slot)
        {
            if (_activeMarkers.ContainsKey(slot))
            {
                return;
            }

            GameObject marker = _pool.Count > 0 ? _pool.Pop() : CreateMarker();
            marker.transform.position = slot.Position.ToUnity() + (Vector3.up * LiftMetres);
            marker.SetActive(true);
            _activeMarkers.Add(slot, marker);
        }

        private void HandleSlotReleased(GarrisonSlot slot)
        {
            if (!_activeMarkers.TryGetValue(slot, out GameObject marker))
            {
                return;
            }

            _activeMarkers.Remove(slot);
            marker.SetActive(false);
            _pool.Push(marker);
        }

        private GameObject CreateMarker()
        {
            GameObject marker = new GameObject(MarkerName);
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = new Vector3(DiameterMetres, ThicknessMetres / CylinderMeshHeightMetres, DiameterMetres);
            marker.AddComponent<MeshFilter>().sharedMesh = PrimitiveMeshes.Cylinder;
            MeshRenderer meshRenderer = marker.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            return marker;
        }
    }
}
