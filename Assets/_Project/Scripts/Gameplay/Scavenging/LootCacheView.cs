using System;
using Stilargo.Core.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.Scavenging
{
    /// <summary>Shows one loot cache in the world and removes it once it is looted.</summary>
    public sealed class LootCacheView : MonoBehaviour
    {
        private LootCache _cache;

        public void Construct(LootCache cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            if (_cache != null)
            {
                throw new InvalidOperationException($"{nameof(LootCacheView)} is already constructed.");
            }

            _cache = cache;
            _cache.Looted += HandleLooted;
        }

        // The cache outlives this view, so the subscription must not keep a destroyed object alive.
        private void OnDestroy()
        {
            if (_cache == null)
            {
                return;
            }

            _cache.Looted -= HandleLooted;
        }

        private void HandleLooted()
        {
            Destroy(gameObject);
        }
    }
}
