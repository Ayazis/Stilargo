namespace Stilargo.Editor.World
{
    /// <summary>How a textured <see cref="HouseMaterialLook"/> puts its texture set on the mesh.</summary>
    public enum SurfaceMapping
    {
        /// <summary>Triplanar in world space, for scaled boxes whose own UVs would stretch.</summary>
        Projected,

        /// <summary>Through UVs in metres, with parallax depth, for relief that must follow each face (roof tiles).</summary>
        MetricUvRelief,

        /// <summary>Through the scan's own UVs, double-sided, for scanned foliage whose leaves are single sheets.</summary>
        ScannedFoliage
    }
}
