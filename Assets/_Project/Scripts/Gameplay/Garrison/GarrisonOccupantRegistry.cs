using System;
using System.Collections.Generic;
using Stilargo.Core.Selection;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Finds the garrison occupant behind a selected unit.</summary>
    public sealed class GarrisonOccupantRegistry
    {
        private readonly Dictionary<ISelectable, GarrisonOccupant> _occupants = new Dictionary<ISelectable, GarrisonOccupant>();

        public void Add(ISelectable unit, GarrisonOccupant occupant)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            if (occupant == null)
            {
                throw new ArgumentNullException(nameof(occupant));
            }

            _occupants.Add(unit, occupant);
        }

        public bool Remove(ISelectable unit)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            return _occupants.Remove(unit);
        }

        public bool TryGet(ISelectable unit, out GarrisonOccupant occupant)
        {
            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            return _occupants.TryGetValue(unit, out occupant) && occupant != null;
        }
    }
}
