using System;
using System.Collections.Generic;
using Stilargo.Core.Cover;
using Stilargo.Gameplay.Units;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stilargo.Gameplay.Cover
{
    /// <summary>
    /// Shows a low sandbag wall at every cover point. Render-only (no colliders) so raycasts stay unaffected;
    /// the placeholder material can be swapped for an authored PBR asset at any time.
    /// </summary>
    public sealed class CoverPointView : MonoBehaviour
    {
        private const float WidthMetres = 1.2f;
        private const float HeightMetres = 0.5f;
        private const float DepthMetres = 0.35f;
        private const float Half = 0.5f;

        // Shifts the wall from the point toward the building so a unit standing on the point is shielded.
        private const float OffsetTowardWallMetres = 0.45f;
        private const string ObjectName = "CoverObject";

        [SerializeField] private Material _material;

        private bool _isConstructed;

        public void Construct(IReadOnlyList<CoverPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            if (_material == null)
            {
                throw new InvalidOperationException("CoverPointView needs a material.");
            }

            if (_isConstructed)
            {
                throw new InvalidOperationException("CoverPointView is already constructed.");
            }

            _isConstructed = true;
            foreach (CoverPoint point in points)
            {
                Spawn(point);
            }
        }

        private void Spawn(CoverPoint point)
        {
            Vector3 normal = point.Normal.ToUnity();
            Vector3 ground = point.Position.ToUnity();
            ground.y = 0f;
            GameObject cover = new GameObject(ObjectName);
            cover.transform.SetParent(transform, false);
            cover.transform.position = ground - (normal * OffsetTowardWallMetres) + (Vector3.up * (HeightMetres * Half));
            cover.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
            cover.transform.localScale = new Vector3(WidthMetres, HeightMetres, DepthMetres);
            cover.AddComponent<MeshFilter>().sharedMesh = PrimitiveMeshes.Cube;
            MeshRenderer meshRenderer = cover.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = _material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.On;
            meshRenderer.receiveShadows = true;
        }
    }
}
