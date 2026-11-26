namespace Stilargo.Core.City
{
    /// <summary>One block of the town plan: the cells between its streets and what it is laid out as.</summary>
    public readonly struct TownBlock
    {
        public TownBlock(GridRect area, BlockRole role)
        {
            Area = area;
            Role = role;
        }

        public GridRect Area { get; }

        public BlockRole Role { get; }

        public TownBlock As(BlockRole role)
        {
            return new TownBlock(Area, role);
        }
    }
}
