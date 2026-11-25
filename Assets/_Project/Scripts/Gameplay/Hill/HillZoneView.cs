using System;
using System.Linq;
using Stilargo.Core.Hill;
using Stilargo.Gameplay.Units;
using UnityEngine;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>
    /// A flat greybox disc that marks the hill: it is placed and sized from the config at startup and shows who
    /// holds the hill by swapping materials.
    /// </summary>
    public sealed class HillZoneView : MonoBehaviour
    {
        private const float DiameterPerRadius = 2f;

        [SerializeField] private Renderer _marker;
        [SerializeField] private float _thicknessMetres = 0.02f;
        // Just above the road, so the disc doesn't z-fight with it.
        [SerializeField] private float _heightAboveGroundMetres = 0.03f;
        [SerializeField] private HillStateLook[] _looks = Array.Empty<HillStateLook>();

        private HillControl _hill;
        private bool _isSubscribed;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Construct(HillControl hill)
        {
            if (_marker == null)
            {
                throw new InvalidOperationException($"{nameof(HillZoneView)} on '{name}' has no marker renderer.");
            }

            if (_hill != null)
            {
                throw new InvalidOperationException($"{nameof(HillZoneView)} is already constructed.");
            }

            _hill = hill ?? throw new ArgumentNullException(nameof(hill));
            PlaceMarker(hill.Zone);
            Show(hill.State);
            Subscribe();
        }

        private void PlaceMarker(HillZone zone)
        {
            Vector3 center = zone.Center.ToUnity();
            float diameter = zone.RadiusMetres * DiameterPerRadius;
            _marker.transform.position = new Vector3(center.x, _heightAboveGroundMetres, center.z);
            _marker.transform.localScale = new Vector3(diameter, _thicknessMetres, diameter);
        }

        private void Show(HillState state)
        {
            HillStateLook look = _looks.FirstOrDefault(candidate => candidate.State == state);
            if (look == null || look.Material == null)
            {
                throw new InvalidOperationException($"{nameof(HillZoneView)} on '{name}' has no material for {state}.");
            }

            _marker.sharedMaterial = look.Material;
        }

        private void Subscribe()
        {
            if (_hill == null || _isSubscribed)
            {
                return;
            }

            _hill.StateChanged += Show;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _hill.StateChanged -= Show;
            _isSubscribed = false;
        }
    }
}
