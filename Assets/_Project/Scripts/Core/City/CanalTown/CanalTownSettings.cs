using System;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>Tunable shape of the canal town. <see cref="Validate"/> throws on values the generator can't use.</summary>
    public sealed class CanalTownSettings
    {
        /// <summary>Whole map, town plus countryside.</summary>
        public int WidthCells { get; set; } = 136;

        public int HeightCells { get; set; } = 136;

        public float CellSizeMetres { get; set; } = 2f;

        /// <summary>Countryside between the map edge and the town's ring road.</summary>
        public int CountrysideCells { get; set; } = 20;

        public int MinParcelCells { get; set; } = 8;

        public int MaxParcelCells { get; set; } = 16;

        public int FarmhouseCells { get; set; } = 4;

        public int FarmyardCells { get; set; } = 8;

        public int StreetWidthCells { get; set; } = 4;

        /// <summary>Width of the high road that leads from the map edge, where the soldiers enter, to the town.</summary>
        public int HighRoadWidthCells { get; set; } = 4;

        public int CanalWidthCells { get; set; } = 5;

        /// <summary>Where the canal runs, as a share of the town's height measured from the south.</summary>
        public float CanalPositionShare { get; set; } = 0.3f;

        public int BridgeCount { get; set; } = 3;

        public int MinBlockCells { get; set; } = 9;

        public int MaxBlockCells { get; set; } = 15;

        public int HouseDepthCells { get; set; } = 4;

        public int MinFrontageCells { get; set; } = 3;

        public int MaxFrontageCells { get; set; } = 5;

        public int MinStoreys { get; set; } = 2;

        public int MaxStoreys { get; set; } = 3;

        /// <summary>Extra storeys for houses on a corner or facing a square.</summary>
        public int ProminentExtraStoreys { get; set; } = 1;

        public int ParkCount { get; set; } = 2;

        /// <summary>Share of the blocks on the town's edge that hold free-standing homes in gardens instead of rows.</summary>
        public float DetachedEdgeShare { get; set; } = 0.5f;

        /// <summary>Size of one detached home's lot, garden included.</summary>
        public int DetachedLotCells { get; set; } = 7;

        public void Validate()
        {
            Require(WidthCells - (CountrysideCells * 2) >= 40 && HeightCells - (CountrysideCells * 2) >= 40, "The town needs at least 40×40 cells.");
            Require(CellSizeMetres > 0f, "Cell size must be positive.");
            Require(CountrysideCells >= 0, "Countryside width can't be negative.");
            Require(MinParcelCells >= FarmyardCells && MaxParcelCells >= MinParcelCells, "Parcel sizes are invalid.");
            Require(FarmyardCells > FarmhouseCells && FarmhouseCells >= 1, "A farmyard must be larger than its farmhouse.");
            Require(HighRoadWidthCells >= 1 && StreetWidthCells >= 1 && CanalWidthCells >= 1, "Streets and canal need a width.");
            Require(CanalPositionShare > 0f && CanalPositionShare < 1f, "Canal position must be between 0 and 1.");
            Require(BridgeCount >= 1, "The canal needs at least one bridge.");
            Require(MinBlockCells >= (HouseDepthCells * 2) + 1 && MaxBlockCells >= MinBlockCells, "Block sizes are invalid.");
            Require(MinFrontageCells >= 1 && MaxFrontageCells >= MinFrontageCells, "Frontages are invalid.");
            Require(MinStoreys >= 1 && MaxStoreys >= MinStoreys && ProminentExtraStoreys >= 0, "Storeys are invalid.");
            Require(DetachedEdgeShare >= 0f && DetachedEdgeShare <= 1f, "Detached share must be between 0 and 1.");
            Require(DetachedLotCells >= 4, "A detached lot needs at least 4 cells for a house and its garden.");
            Require(ParkCount >= 0, "Park count can't be negative.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
