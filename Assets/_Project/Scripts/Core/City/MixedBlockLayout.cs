using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>
    /// An outskirts neighbourhood: the block cut into plots, most holding a house of the neighbourhood's height and
    /// some left as open ground (villa gardens, workshop yards).
    /// </summary>
    public sealed class MixedBlockLayout : IBlockLayout
    {
        private const int OpenGroundStoreys = 1;

        private readonly PlotSplitter _splitter;
        private readonly IntRange _storeys;
        private readonly OpenGroundShare _open;

        public MixedBlockLayout(PlotSplitter splitter, IntRange storeys, OpenGroundShare open)
        {
            _splitter = splitter ?? throw new ArgumentNullException(nameof(splitter));
            if (storeys.Min <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(storeys), storeys, "Storeys must be positive.");
            }

            _storeys = storeys;
            _open = open;
        }

        public void Lay(GridRect block, Random random, ICollection<Plot> plots)
        {
            if (random == null || plots == null)
            {
                throw new ArgumentNullException(random == null ? nameof(random) : nameof(plots));
            }

            var areas = new List<GridRect>();
            _splitter.Split(block, random, areas);
            foreach (GridRect area in areas)
            {
                plots.Add(random.NextDouble() < _open.Value
                    ? new Plot(area, OpenGroundStoreys, _open.Use)
                    : new Plot(area, _storeys.Pick(random)));
            }
        }
    }
}
