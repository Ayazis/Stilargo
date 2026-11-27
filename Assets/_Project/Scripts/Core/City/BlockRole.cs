namespace Stilargo.Core.City
{
    /// <summary>
    /// What a whole city block is laid out as, from the market square in the middle outwards. The town plan first
    /// marks the outer blocks as <see cref="Outskirts"/>; each then becomes one of the neighbourhoods after it.
    /// </summary>
    public enum BlockRole
    {
        MarketSquare,
        Park,
        HouseBlock,
        Outskirts,
        OldTown,
        Villas,
        Workshops,
        Plaza
    }
}
