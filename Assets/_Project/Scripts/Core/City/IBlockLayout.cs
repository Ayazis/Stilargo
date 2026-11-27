using System;
using System.Collections.Generic;

namespace Stilargo.Core.City
{
    /// <summary>Fills one city block with plots in the style of its <see cref="BlockRole"/>.</summary>
    public interface IBlockLayout
    {
        void Lay(GridRect block, Random random, ICollection<Plot> plots);
    }
}
