using System;
using Stilargo.Core.City.CanalTown;

namespace Stilargo.Core.City
{
    /// <summary>
    /// The town plan, loaded from <c>Town.json</c>: a market square in the middle, house blocks around it,
    /// varied neighbourhoods on the outskirts with irregular streets, parks, and life on the streets.
    /// </summary>
    public sealed class TownSettings
    {
        public const int DefaultParkBlockCount = 2;

        public TownSettings()
            : this(DefaultParkBlockCount, new RowHouseSettings())
        {
        }

        public TownSettings(int parkBlockCount, RowHouseSettings houseBlock)
        {
            if (parkBlockCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(parkBlockCount), parkBlockCount, "Park block count must not be negative.");
            }

            ParkBlockCount = parkBlockCount;
            HouseBlock = houseBlock ?? throw new ArgumentNullException(nameof(houseBlock));
        }

        /// <summary>How many blocks become a park. Outskirts blocks are used first.</summary>
        public int ParkBlockCount { get; }

        public RowHouseSettings HouseBlock { get; }

        public StreetSettings Streets { get; private set; } = new StreetSettings();

        public NeighbourhoodSettings Neighbourhoods { get; private set; } = new NeighbourhoodSettings();

        public StreetLifeSettings StreetLife { get; private set; } = new StreetLifeSettings();

        /// <summary>The canal town the game plays in; the settings above shape the older grid town.</summary>
        public CanalTownSettings CanalTown { get; private set; } = new CanalTownSettings();

        public TownSettings WithCanalTown(CanalTownSettings canalTown)
        {
            CanalTown = canalTown ?? throw new ArgumentNullException(nameof(canalTown));
            CanalTown.Validate();
            return this;
        }

        public TownSettings WithStreets(StreetSettings streets)
        {
            Streets = (streets ?? throw new ArgumentNullException(nameof(streets))).Validate();
            return this;
        }

        public TownSettings WithNeighbourhoods(NeighbourhoodSettings neighbourhoods)
        {
            Neighbourhoods = (neighbourhoods ?? throw new ArgumentNullException(nameof(neighbourhoods))).Validate();
            return this;
        }

        public TownSettings WithStreetLife(StreetLifeSettings streetLife)
        {
            StreetLife = (streetLife ?? throw new ArgumentNullException(nameof(streetLife))).Validate();
            return this;
        }
    }
}
