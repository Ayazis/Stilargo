using System;
using Stilargo.Core.Combat;
using UnityEngine;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>The shape of <c>Combat.json</c>. Defaults match a new <see cref="CombatSettings"/>.</summary>
    [Serializable]
    public sealed class CombatConfig
    {
        public const string FileName = "Combat.json";

        private const string PlayerSection = "player";
        private const string EnemySection = "enemy";

        // JsonUtility maps field names to JSON keys, so these fields are named like the keys instead of _camelCase.
        [SerializeField] private UnitSideEntry player = UnitSideEntry.DefaultsFor(new CombatSettings().Player);
        [SerializeField] private UnitSideEntry enemy = UnitSideEntry.DefaultsFor(new CombatSettings().Enemy);
        [SerializeField] private float playerMoveSpeedMetresPerSecond = CombatSettings.DefaultPlayerMoveSpeedMetresPerSecond;
        [SerializeField] private float corpseSeconds = CombatSettings.DefaultCorpseSeconds;

        public static CombatSettings Load()
        {
            return ConfigFile.Read<CombatConfig>(FileName).ToSettings();
        }

        public CombatSettings ToSettings()
        {
            try
            {
                return new CombatSettings()
                    .WithSides(player.ToSettings(PlayerSection), enemy.ToSettings(EnemySection))
                    .WithPlayerMoveSpeed(playerMoveSpeedMetresPerSecond)
                    .WithCorpseTime(corpseSeconds);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException($"{FileName} is invalid: {exception.Message}", exception);
            }
        }
    }
}
