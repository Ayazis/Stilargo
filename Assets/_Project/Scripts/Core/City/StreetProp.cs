namespace Stilargo.Core.City
{
    /// <summary>One prop on a road cell, pushed against the plot that lies one cell in the <see cref="Towards"/> direction.</summary>
    public readonly struct StreetProp
    {
        public StreetProp(StreetPropKind kind, GridPosition cell, GridPosition towards)
        {
            Kind = kind;
            Cell = cell;
            Towards = towards;
        }

        public StreetPropKind Kind { get; }

        public GridPosition Cell { get; }

        /// <summary>Unit step (one of the four directions) from the road cell to the plot beside it.</summary>
        public GridPosition Towards { get; }
    }
}
