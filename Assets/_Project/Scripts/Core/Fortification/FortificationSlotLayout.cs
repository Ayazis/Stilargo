using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.City;
using Stilargo.Core.Garrison;

namespace Stilargo.Core.Fortification
{
    /// <summary>
    /// Derives a house's fortification slots from its garrison layout: one entrance slot at the street door,
    /// one window slot per garrison window and one roof slot per garrison roof spot. Sharing the spots keeps
    /// fortifications where defenders actually stand.
    /// </summary>
    public sealed class FortificationSlotLayout
    {
        private const float Half = 0.5f;

        private readonly float _cellSize;

        public FortificationSlotLayout(float cellSize)
        {
            if (!(cellSize > 0f) || float.IsInfinity(cellSize))
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "Cell size must be positive and finite.");
            }

            _cellSize = cellSize;
        }

        public IReadOnlyList<FortificationSlot> Create(GarrisonBuilding building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            List<FortificationSlot> slots = new List<FortificationSlot>
            {
                new FortificationSlot(FortificationSlotKind.Entrance, building.Entrance, EntranceFacing(building)),
            };

            foreach (GarrisonSlot garrisonSlot in building.Slots)
            {
                slots.Add(new FortificationSlot(KindOf(garrisonSlot.Kind), garrisonSlot.Position, garrisonSlot.Facing));
            }

            return slots;
        }

        private static FortificationSlotKind KindOf(GarrisonSlotKind kind)
        {
            return kind == GarrisonSlotKind.Roof ? FortificationSlotKind.Roof : FortificationSlotKind.Window;
        }

        // The entrance lies on the road next to the plot, so pointing from the plot centre to it faces the street.
        // Snapped to the dominant axis because the door is in an axis-aligned wall.
        private Vector3 EntranceFacing(GarrisonBuilding building)
        {
            GridRect area = building.Plot.Area;
            Vector3 centre = new Vector3(
                (area.Min.X + (area.Width * Half)) * _cellSize,
                0f,
                (area.Min.Y + (area.Height * Half)) * _cellSize);
            Vector3 offset = building.Entrance - centre;
            if (Math.Abs(offset.X) >= Math.Abs(offset.Z))
            {
                return new Vector3(Math.Sign(offset.X), 0f, 0f);
            }

            return new Vector3(0f, 0f, Math.Sign(offset.Z));
        }
    }
}
