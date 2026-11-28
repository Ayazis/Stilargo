using System;

namespace Stilargo.Core.City
{
    /// <summary>Where the showcase house stands: the open plot it takes over and the house's own footprint inside it.</summary>
    public sealed class ShowcaseSite
    {
        public ShowcaseSite(Plot replaced, Plot house)
        {
            Replaced = replaced ?? throw new ArgumentNullException(nameof(replaced));
            House = house ?? throw new ArgumentNullException(nameof(house));
        }

        /// <summary>The field or meadow the house is built on; it is not dressed.</summary>
        public Plot Replaced { get; }

        public Plot House { get; }
    }
}
