namespace Stilargo.Gameplay.City
{
    /// <summary>How far roofs reach past the walls and how steep they are.</summary>
    internal readonly struct RoofShape
    {
        public RoofShape(float overhangMetres, float risePerHalfSpan)
        {
            OverhangMetres = overhangMetres;
            RisePerHalfSpan = risePerHalfSpan;
        }

        public float OverhangMetres { get; }

        /// <summary>Roof rise per metre of half the shorter span; 0.5 is a 26.6° pitch.</summary>
        public float RisePerHalfSpan { get; }
    }
}
