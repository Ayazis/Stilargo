using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>Reveals fog around this object's transform while enabled.</summary>
    public sealed class VisionSourceComponent : MonoBehaviour, IVisionSource
    {
        [SerializeField] private float _visionRadius = 10f;

        private VisibilityGrid _grid;
        private Transform _transform;

        public WorldPoint Position => new WorldPoint(_transform.position.x, _transform.position.z);

        public float VisionRadius => _visionRadius;

        public void Construct(VisibilityGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (_grid != null)
            {
                throw new InvalidOperationException($"{nameof(VisionSourceComponent)} is already constructed.");
            }

            _grid = grid;
            CacheComponents();
            if (isActiveAndEnabled)
            {
                _grid.Register(this);
            }
        }

        private void Awake()
        {
            CacheComponents();
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_transform == null)
            {
                _transform = transform;
            }
        }

        private void OnEnable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Register(this);
        }

        private void OnDisable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Unregister(this);
        }
    }
}
