using System;
using System.Collections.Generic;

namespace Stilargo.Core.City.CanalTown
{
    /// <summary>Where the canal and its quays lie, and the x of each bridge's west edge.</summary>
    public sealed class Canal
    {
        public Canal(GridRect water, int southEdgeY, int northEdgeY, IReadOnlyList<int> bridges, int bridgeWidthCells)
        {
            if (bridgeWidthCells <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bridgeWidthCells), bridgeWidthCells, "Bridges must be at least one cell wide.");
            }

            Water = water;
            SouthEdgeY = southEdgeY;
            NorthEdgeY = northEdgeY;
            Bridges = bridges ?? throw new ArgumentNullException(nameof(bridges));
            BridgeWidthCells = bridgeWidthCells;
        }

        /// <summary>The canal's water inside the town and under the ring road.</summary>
        public GridRect Water { get; }

        /// <summary>First row of the south quay; blocks south of the canal end here.</summary>
        public int SouthEdgeY { get; }

        /// <summary>First row past the north quay; blocks north of the canal start here.</summary>
        public int NorthEdgeY { get; }

        public IReadOnlyList<int> Bridges { get; }

        /// <summary>Width of each bridge (and the street it carries) in cells.</summary>
        public int BridgeWidthCells { get; }
    }
}
