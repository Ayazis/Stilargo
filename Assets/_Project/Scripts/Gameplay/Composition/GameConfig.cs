using System;
using Stilargo.Core.Fortification;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Settings loaded from the JSON files in <c>StreamingAssets/Config</c>, one property per feature file.
    /// The bootstrapper loads them; installers read what they need from <see cref="GameContext.Config"/>.
    /// </summary>
    public sealed class GameConfig
    {
        public GameConfig(FortificationSettings fortifications)
        {
            Fortifications = fortifications ?? throw new ArgumentNullException(nameof(fortifications));
        }

        public FortificationSettings Fortifications { get; }
    }
}
