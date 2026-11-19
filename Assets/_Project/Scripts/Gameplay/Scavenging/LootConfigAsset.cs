using Stilargo.Core.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.Scavenging
{
    /// <summary>Designer-facing data for <see cref="LootConfig"/>. Validation lives in Core.</summary>
    [CreateAssetMenu(fileName = "LootConfig", menuName = "Stilargo/Loot Config")]
    public sealed class LootConfigAsset : ScriptableObject
    {
        [SerializeField] private int _cacheCount = 12;
        [SerializeField] private int _minValue = 10;
        [SerializeField] private int _maxValue = 50;

        public LootConfig ToConfig()
        {
            LootConfig config = new LootConfig
            {
                CacheCount = _cacheCount,
                MinValue = _minValue,
                MaxValue = _maxValue,
            };
            config.Validate();
            return config;
        }
    }
}
