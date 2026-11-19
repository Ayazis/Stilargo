using System;
using System.Collections.Generic;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;
using Stilargo.Gameplay.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Scavenging
{
    /// <summary>Spawns a view for every loot cache. Caches hide in the fog until a unit sees them.</summary>
    public sealed class LootCacheSpawner : MonoBehaviour
    {
        [SerializeField] private LootCacheView _cachePrefab;
        [SerializeField] private float _heightAboveGround = 0.5f;

        public void Construct(IReadOnlyList<LootCache> caches, VisibilityGrid visibilityGrid)
        {
            if (caches == null)
            {
                throw new ArgumentNullException(nameof(caches));
            }

            if (visibilityGrid == null)
            {
                throw new ArgumentNullException(nameof(visibilityGrid));
            }

            if (_cachePrefab == null)
            {
                throw new InvalidOperationException($"{nameof(LootCacheSpawner)} needs a cache prefab.");
            }

            foreach (LootCache cache in caches)
            {
                Spawn(cache, visibilityGrid);
            }
        }

        private void Spawn(LootCache cache, VisibilityGrid visibilityGrid)
        {
            Vector3 position = new Vector3(cache.Position.X, _heightAboveGround, cache.Position.Z);
            LootCacheView view = Instantiate(_cachePrefab, position, Quaternion.identity, transform);
            view.Construct(cache);

            FogHideable hideable = view.GetComponent<FogHideable>();
            if (hideable != null)
            {
                hideable.Construct(visibilityGrid);
            }
        }
    }
}
