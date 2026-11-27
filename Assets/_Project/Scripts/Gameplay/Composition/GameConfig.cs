using System;
using Stilargo.Core.City;
using Stilargo.Core.Combat;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using AudioSettings = Stilargo.Core.Audio.AudioSettings;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Income;
using Stilargo.Core.Rival;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Settings loaded from the JSON files in <c>StreamingAssets/Config</c>, one property per feature file section.
    /// The bootstrapper loads them; installers read what they need from <see cref="GameContext.Config"/>.
    /// </summary>
    public sealed class GameConfig
    {
        public GameConfig(
            FortificationSettings fortifications,
            HillRivalSettings hillRival,
            EconomySettings economy,
            HouseIncomeSettings houseIncome)
        {
            Fortifications = fortifications ?? throw new ArgumentNullException(nameof(fortifications));
            HouseIncome = houseIncome ?? throw new ArgumentNullException(nameof(houseIncome));
            if (hillRival == null)
            {
                throw new ArgumentNullException(nameof(hillRival));
            }

            Economy = economy ?? throw new ArgumentNullException(nameof(economy));
            Hill = hillRival.Hill;
            Rival = hillRival.Rival;
        }

        /// <summary>From <c>Fortification.json</c>.</summary>
        public FortificationSettings Fortifications { get; }

        /// <summary>From the <c>hill</c> section of <c>Hill.json</c>.</summary>
        public HillSettings Hill { get; }

        /// <summary>From <c>Economy.json</c>.</summary>
        public EconomySettings Economy { get; }

        /// <summary>From the <c>rival</c> section of <c>Hill.json</c>.</summary>
        public RivalSettings Rival { get; }

        /// <summary>From <c>Income.json</c>.</summary>
        public HouseIncomeSettings HouseIncome { get; }

        /// <summary>From <c>Combat.json</c>; defaults until <see cref="WithCombat"/> is called.</summary>
        public CombatSettings Combat { get; private set; } = new CombatSettings();

        /// <summary>From <c>Vision.json</c>; defaults until <see cref="WithVision"/> is called.</summary>
        public VisionSettings Vision { get; private set; } = new VisionSettings();

        /// <summary>From <c>Buildings.json</c>; defaults until <see cref="WithDistrict"/> is called.</summary>
        public DistrictSettings District { get; private set; } = new DistrictSettings();

        /// <summary>From <c>Town.json</c>; defaults until <see cref="WithTown"/> is called.</summary>
        public TownSettings Town { get; private set; } = new TownSettings();

        /// <summary>From <c>Audio.json</c>; defaults until <see cref="WithAudio"/> is called.</summary>
        public AudioSettings Audio { get; private set; } = new AudioSettings();

        public GameConfig WithCombat(CombatSettings combat)
        {
            Combat = combat ?? throw new ArgumentNullException(nameof(combat));
            return this;
        }

        public GameConfig WithVision(VisionSettings vision)
        {
            Vision = vision ?? throw new ArgumentNullException(nameof(vision));
            return this;
        }

        public GameConfig WithDistrict(DistrictSettings district)
        {
            District = district ?? throw new ArgumentNullException(nameof(district));
            return this;
        }

        public GameConfig WithTown(TownSettings town)
        {
            Town = town ?? throw new ArgumentNullException(nameof(town));
            return this;
        }

        public GameConfig WithAudio(AudioSettings audio)
        {
            Audio = audio ?? throw new ArgumentNullException(nameof(audio));
            return this;
        }
    }
}
