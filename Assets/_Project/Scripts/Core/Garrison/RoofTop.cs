using Stilargo.Core.City;

namespace Stilargo.Core.Garrison
{
    /// <summary>The flat top units stand on: a building's roof, or its bell tower's top when it has one.</summary>
    internal readonly struct RoofTop
    {
        public RoofTop(GridRect area, int storeys)
        {
            Area = area;
            Storeys = storeys;
        }

        public GridRect Area { get; }

        /// <summary>Storeys below the top; the top stands this many storey heights above the street.</summary>
        public int Storeys { get; }
    }
}
