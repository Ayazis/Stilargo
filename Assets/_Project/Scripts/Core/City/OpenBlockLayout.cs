using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>A block that is one open space: the market square, a plaza or a park.</summary>
    public sealed class OpenBlockLayout : IBlockLayout
    {
        private const int OpenGroundStoreys = 1;

        private readonly PlotUse _use;

        public OpenBlockLayout(PlotUse use)
        {
            _use = use;
        }

        public void Lay(GridRect block, Random random, ICollection<Plot> plots)
        {
            if (plots == null)
            {
                throw new ArgumentNullException(nameof(plots));
            }

            plots.Add(new Plot(block, OpenGroundStoreys, _use));
        }
    }
}
