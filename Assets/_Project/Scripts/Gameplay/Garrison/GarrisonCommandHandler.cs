using System;
using System.Collections.Generic;
using Stilargo.Core.Garrison;
using Stilargo.Core.Input;
using Stilargo.Core.Selection;
using Stilargo.Gameplay.Orders;
using UnityEngine;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Turns a double right-click on a building into an order to garrison it; a single click is a plain move.</summary>
    public sealed class GarrisonCommandHandler : IPointerCommandHandler
    {
        // A wall hit sits exactly on the footprint edge; nudging along the ray keeps the lookup inside the building.
        private const float FootprintNudgeMetres = 0.05f;

        private readonly GarrisonDirectory _directory;
        private readonly GarrisonOccupantRegistry _occupants;

        // One right-click on a building only moves the squad there; a second one in quick succession sends it in.
        private readonly DoubleClick _doubleClick = new DoubleClick(DoubleClick.DefaultWindowSeconds);

        public GarrisonCommandHandler(GarrisonDirectory directory, GarrisonOccupantRegistry occupants)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _occupants = occupants ?? throw new ArgumentNullException(nameof(occupants));
        }

        public bool TryHandle(Ray pointerRay, IReadOnlyList<ISelectable> selected)
        {
            if (selected == null)
            {
                throw new ArgumentNullException(nameof(selected));
            }

            if (!Physics.Raycast(pointerRay, out RaycastHit hit))
            {
                return false;
            }

            Vector3 point = hit.point + (pointerRay.direction.normalized * FootprintNudgeMetres);
            if (!_directory.TryFindBuildingAt(point.x, point.z, out GarrisonBuilding building)
                || !_doubleClick.Register(building, Time.unscaledTime))
            {
                return false;
            }

            foreach (ISelectable unit in selected)
            {
                if (_occupants.TryGet(unit, out GarrisonOccupant occupant))
                {
                    occupant.Enter(building);
                }
            }

            return true;
        }
    }
}
