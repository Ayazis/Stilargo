using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>Hides this object's renderers whenever its position is not currently visible.</summary>
    public sealed class FogHideable : MonoBehaviour
    {
        private VisibilityGrid _grid;
        private Renderer[] _renderers;
        private Transform _transform;

        public void Construct(VisibilityGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (_grid != null)
            {
                throw new InvalidOperationException($"{nameof(FogHideable)} is already constructed.");
            }

            _grid = grid;
            CacheComponents();
            if (isActiveAndEnabled)
            {
                _grid.Refreshed += UpdateVisibility;
            }

            UpdateVisibility();
        }

        private void Awake()
        {
            CacheComponents();
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_renderers != null)
            {
                return;
            }

            _transform = transform;
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Refreshed += UpdateVisibility;
            UpdateVisibility();
        }

        private void OnDisable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Refreshed -= UpdateVisibility;
        }

        private void UpdateVisibility()
        {
            Vector3 position = _transform.position;
            bool isVisible = _grid.IsVisible(new WorldPoint(position.x, position.z));
            foreach (Renderer childRenderer in _renderers)
            {
                childRenderer.enabled = isVisible;
            }
        }
    }
}
