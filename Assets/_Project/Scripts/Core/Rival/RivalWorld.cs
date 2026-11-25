using System;

namespace Stilargo.Core.Rival
{
    /// <summary>Everything a rival strategy acts on: its squad, the world around it and its tuning.</summary>
    public sealed class RivalWorld
    {
        public RivalWorld(RivalSquad squad, RivalSurroundings surroundings, RivalSettings settings)
        {
            Squad = squad ?? throw new ArgumentNullException(nameof(squad));
            Surroundings = surroundings ?? throw new ArgumentNullException(nameof(surroundings));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public RivalSquad Squad { get; }

        public RivalSurroundings Surroundings { get; }

        public RivalSettings Settings { get; }
    }
}
