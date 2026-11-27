using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>
    /// Reveals fog around this object's transform while enabled. It sees further the higher it stands, so a unit
    /// on a roof or a tower overlooks the streets around it.
    /// </summary>
    public sealed class VisionSourceComponent : MonoBehaviour, IVisionSource
    {
        // The unit's pivot is the centre of its capsule, a metre above its feet.
        private const float DefaultPivotHeightMetres = 1f;

        [SerializeField, Min(0f)] private float _pivotHeightMetres = DefaultPivotHeightMetres;

        private VisibilityGrid _grid;
        private VisionSettings _settings;
        private Transform _transform;

        public WorldPoint Position => new WorldPoint(_transform.position.x, _transform.position.z);

        public float VisionRadius => _settings.RadiusAt(FeetHeight, 0f);

        public float EyeHeightMetres => FeetHeight + _settings.EyeHeightMetres;

        private float FeetHeight => _transform.position.y - _pivotHeightMetres;

        public void Construct(VisibilityGrid grid, VisionSettings settings)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (_grid != null)
            {
                throw new InvalidOperationException($"{nameof(VisionSourceComponent)} is already constructed.");
            }

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
