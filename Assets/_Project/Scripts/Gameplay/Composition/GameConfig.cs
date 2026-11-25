using System;
using Stilargo.Core.Economy;
using Stilargo.Core.Fortification;
using Stilargo.Core.Hill;
using Stilargo.Core.Rival;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Settings loaded from the JSON files in <c>StreamingAssets/Config</c>, one property per feature file section.
    /// The bootstrapper loads them; installers read what they need from <see cref="GameContext.Config"/>.
    /// </summary>
    public sealed class GameConfig
    {
        public GameConfig(FortificationSettings fortifications, HillRivalSettings hillRival, EconomySettings economy)
        {
            Fortifications = fortifications ?? throw new ArgumentNullException(nameof(fortifications));
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
    }
}
