using Stilargo.Core.City;

namespace Stilargo.Core.Cover
{
    /// <summary>One side of a road cell: the cell and the unit grid step towards the neighbour across that side.</summary>
    internal readonly struct RoadEdge
    {
        public RoadEdge(GridPosition road, GridPosition direction)
        {
            Road = road;
            Direction = direction;
        }

        public GridPosition Road { get; }

        public GridPosition Direction { get; }
    }
}
