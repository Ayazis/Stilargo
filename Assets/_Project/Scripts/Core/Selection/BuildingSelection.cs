using System;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Selection
{
    /// <summary>The one building the player has clicked on, if any. The claim-and-fortify panel acts on it.</summary>
    public sealed class BuildingSelection
    {
        public event Action Changed;

        /// <summary>The selected building, or null when none is.</summary>
        public GarrisonBuilding Selected { get; private set; }

        public bool HasSelection => Selected != null;

        public void Select(GarrisonBuilding building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (building == Selected)
            {
                return;
            }

            Selected = building;
            Changed?.Invoke();
        }

        public void Clear()
        {
            if (Selected == null)
            {
                return;
            }

            Selected = null;
            Changed?.Invoke();
        }
    }
}
