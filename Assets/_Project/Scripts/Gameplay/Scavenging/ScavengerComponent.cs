using System;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.Scavenging
{
    /// <summary>Lets the unit it sits on pick up loot and deliver it home just by getting close enough.</summary>
    public sealed class ScavengerComponent : MonoBehaviour
    {
        private ScavengingArea _area;
        private Transform _transform;

        public event Action<ScavengeResult> Scavenged;

        public LootCarrier Carrier { get; } = new LootCarrier();

        public void Construct(ScavengingArea area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            if (_area != null)
            {
                throw new InvalidOperationException($"{nameof(ScavengerComponent)} is already constructed.");
            }

            _area = area;
            CacheComponents();
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

        private void Update()
        {
            if (_area == null)
            {
                return;
            }

            Vector3 position = _transform.position;
            ScavengeResult result = _area.Visit(Carrier, new WorldPoint(position.x, position.z));
            if (result == ScavengeResult.Nothing)
            {
                return;
            }

            Scavenged?.Invoke(result);
        }
    }
}
