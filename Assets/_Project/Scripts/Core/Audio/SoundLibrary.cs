using System;
using System.Collections.Generic;

namespace Stilargo.Core.Audio
{
    /// <summary>The recipe behind every <see cref="SoundCue"/>. Add a cue by adding its recipe here.</summary>
    public sealed class SoundLibrary
    {
        private const float PlayerRiflePitch = 1f;
        private const float EnemyRiflePitch = 0.9f;
        private const float TurretPitch = 0.7f;
        private const float NoteGapSeconds = 0.11f;
        private const float StingerGapSeconds = 0.18f;
        private const float C4 = 261.63f;
        private const float EFlat4 = 311.13f;
        private const float G4 = 392f;
        private const float C5 = 523.25f;
        private const float E5 = 659.25f;
        private const float G5 = 783.99f;
        private const float C6 = 1046.5f;

        // Fixed seeds keep every noise-based sound identical from run to run.
        private const int PlayerRifleSeed = 11;
        private const int EnemyRifleSeed = 23;
        private const int TurretSeed = 37;
        private const int ImpactSeed = 41;
        private const int DeathSeed = 53;
        private const int HammerSeed = 67;

        private readonly Dictionary<SoundCue, ISoundRecipe> _recipes;

        public SoundLibrary()
        {
            _recipes = new Dictionary<SoundCue, ISoundRecipe>
            {
                { SoundCue.RifleShot, new GunshotRecipe(PlayerRiflePitch, PlayerRifleSeed) },
                { SoundCue.EnemyRifleShot, new GunshotRecipe(EnemyRiflePitch, EnemyRifleSeed) },
                { SoundCue.TurretShot, new GunshotRecipe(TurretPitch, TurretSeed) },
                { SoundCue.BulletHit, new ImpactRecipe(ImpactSeed) },
                { SoundCue.Death, new DeathRecipe(DeathSeed) },
                { SoundCue.UiClick, new ClickRecipe() },
                { SoundCue.Claim, new ChimeRecipe(new[] { C5, E5, G5 }, NoteGapSeconds) },
                { SoundCue.Build, new HammerRecipe(HammerSeed) },
                { SoundCue.Victory, new ChimeRecipe(new[] { C5, E5, G5, C6 }, StingerGapSeconds) },
                { SoundCue.Defeat, new ChimeRecipe(new[] { G4, EFlat4, C4 }, StingerGapSeconds) },
            };
        }

        public ISoundRecipe RecipeFor(SoundCue cue)
        {
            if (!_recipes.TryGetValue(cue, out ISoundRecipe recipe))
            {
                throw new ArgumentOutOfRangeException(nameof(cue), cue, "No recipe for this sound.");
            }

            return recipe;
        }
    }
}
