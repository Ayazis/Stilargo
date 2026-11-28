namespace Stilargo.Core.City.CanalTown
{
    /// <summary>A countryside parcel between two lanes: its area, which way its strip runs and which side its lane is on.</summary>
    internal sealed class Parcel
    {
        public const int NoLane = 0;

        /// <summary>The lane runs past the parcel's low-X (or low-Y) end.</summary>
        public const int LaneBefore = -1;

        /// <summary>The lane runs past the parcel's high-X (or high-Y) end.</summary>
        public const int LaneAfter = 1;

        public Parcel(GridRect area, bool runsAlongX, int laneSide)
        {
            Area = area;
            RunsAlongX = runsAlongX;
            LaneSide = laneSide;
        }

        public GridRect Area { get; }

        /// <summary>True when the strip of parcels runs west to east, so its lanes run north to south.</summary>
        public bool RunsAlongX { get; }

        public int LaneSide { get; }
    }
}
