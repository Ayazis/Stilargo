namespace Stilargo.Gameplay.City
{
    /// <summary>A baked <see cref="RoofProfile"/>: heights row by row from the min-Z row, NaN where nothing covers a cell.</summary>
    public readonly struct RoofProfileData
    {
        public RoofProfileData(float minX, float minZ, float cellSizeMetres, int columns, float[] heights)
        {
            MinX = minX;
            MinZ = minZ;
            CellSizeMetres = cellSizeMetres;
            Columns = columns;
            Heights = heights;
        }

        public float MinX { get; }

        public float MinZ { get; }

        public float CellSizeMetres { get; }

        public int Columns { get; }

        public float[] Heights { get; }
    }
}
