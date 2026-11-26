namespace Stilargo.Core.City
{
    /// <summary>
    /// Which plots a use goes to: the biggest free ones (landmarks), the biggest of those across the street from
    /// the market square (the biggest anywhere when there is no square), or any free one.
    /// </summary>
    public enum PlotPlacement
    {
        Largest,
        FacingSquare,
        Anywhere
    }
}
