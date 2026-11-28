using System;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>A generated canal town: its layout, its canal and the road cells on the map edge where each side's soldiers enter.</summary>
    public sealed class CanalTown
    {
        public CanalTown(CityLayout layout, Canal canal, (GridPosition Player, GridPosition Rival) entries)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            Canal = canal ?? throw new ArgumentNullException(nameof(canal));
            GridPosition playerEntry = entries.Player;
            GridPosition rivalEntry = entries.Rival;
            PlayerEntry = RequireRoad(layout, playerEntry, nameof(playerEntry));
            RivalEntry = RequireRoad(layout, rivalEntry, nameof(rivalEntry));
        }

        public CityLayout Layout { get; }

        public Canal Canal { get; }

        public GridPosition PlayerEntry { get; }

        public GridPosition RivalEntry { get; }

        private static GridPosition RequireRoad(CityLayout layout, GridPosition entry, string name)
        {
            if (!layout.IsRoad(entry))
            {
                throw new ArgumentException($"The entry {entry} must be on a road.", name);
            }

            return entry;
        }
    }
}
