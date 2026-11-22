using System;
using Stilargo.Core.Fog;
using Stilargo.Core.Garrison;
using Stilargo.Core.Orders;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Dependencies a <see cref="GarrisonOccupant"/> needs, bundled to keep Construct small.</summary>
    public sealed class GarrisonOccupantSetup
    {
        public GarrisonOccupantSetup(GarrisonBonuses bonuses, IUnitOrders orders, VisibilityGrid visibilityGrid)
        {
            Bonuses = bonuses ?? throw new ArgumentNullException(nameof(bonuses));
            Orders = orders ?? throw new ArgumentNullException(nameof(orders));
            VisibilityGrid = visibilityGrid ?? throw new ArgumentNullException(nameof(visibilityGrid));
        }

        public GarrisonBonuses Bonuses { get; }

        public IUnitOrders Orders { get; }

        public VisibilityGrid VisibilityGrid { get; }
    }
}
