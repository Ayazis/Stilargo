using Stilargo.Gameplay.Composition;

namespace Stilargo.Infrastructure.Config
{
    /// <summary>Loads every feature's config file into one <see cref="GameConfig"/>; add new files here.</summary>
    public static class GameConfigLoader
    {
        public static GameConfig Load()
        {
            return new GameConfig(FortificationConfig.Load(), HillConfig.Load(), EconomyConfig.Load(), IncomeConfig.Load())
                .WithCombat(CombatConfig.Load())
                .WithVision(VisionConfig.Load())
                .WithDistrict(BuildingsConfig.Load())
                .WithTown(TownConfig.Load())
                .WithAudio(AudioConfig.Load());
        }
    }
}
