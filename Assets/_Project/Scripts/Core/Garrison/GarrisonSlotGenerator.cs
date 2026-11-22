using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Stilargo.Core.City;

namespace Stilargo.Core.Garrison
{
    /// <summary>
    /// Turns the city's plots into enterable buildings. Only plots touching a road get one, because units
    /// enter from the street. Windows go on road-facing facades only (one row per storey, at floor level,
    /// just outside the wall); roof slots are spread in a grid over the roof top.
    /// </summary>
    public sealed class GarrisonSlotGenerator
    {
        private const float HalfStep = 0.5f;

        private readonly GarrisonLayoutSettings _settings;

        public GarrisonSlotGenerator(GarrisonLayoutSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReadOnlyList<GarrisonBuilding> Generate(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            List<GarrisonBuilding> buildings = new List<GarrisonBuilding>();
            foreach (Plot plot in layout.Plots)
            {
                List<PlotFacade> roadFacades = PlotFacade.AllOf(plot.Area, layout.CellSize)
                    .Where(facade => facade.FacesRoad(layout))
                    .ToList();
                if (roadFacades.Count == 0)
                {
                    continue;
                }

                buildings.Add(CreateBuilding(plot, roadFacades, layout));
            }

            return buildings;
        }

        private GarrisonBuilding CreateBuilding(Plot plot, IReadOnlyList<PlotFacade> roadFacades, CityLayout layout)
        {
            List<GarrisonSlot> slots = CreateRoofSlots(plot, layout.CellSize, roadFacades[0].Outward).ToList();
            for (int storey = 0; storey < plot.Storeys; storey++)
            {
                foreach (PlotFacade facade in roadFacades)
                {
                    slots.AddRange(CreateWindowSlots(facade, storey));
                }
            }

            return new GarrisonBuilding(plot, slots, FindEntrance(roadFacades, layout));
        }

        private IEnumerable<GarrisonSlot> CreateWindowSlots(PlotFacade facade, int storey)
        {
            int count = _settings.WindowsPerFacadePerStorey;
            Vector3 lift = Vector3.UnitY * (storey * _settings.StoreyHeightMetres);
            Vector3 push = facade.Outward * _settings.WindowOffsetMetres;
            for (int index = 0; index < count; index++)
            {
                float along = (index + HalfStep) / count;
                Vector3 position = Vector3.Lerp(facade.Start, facade.End, along) + push + lift;
                yield return new GarrisonSlot(GarrisonSlotKind.Window, storey, new GarrisonSlotPose(position, facade.Outward));
            }
        }

        private IEnumerable<GarrisonSlot> CreateRoofSlots(Plot plot, float cellSize, Vector3 facing)
        {
            int count = _settings.RoofSlotCount;
            if (count == 0)
            {
                yield break;
            }

            int columns = (int)Math.Ceiling(Math.Sqrt(count));
            int rows = (int)Math.Ceiling(count / (double)columns);
            float roofHeight = plot.Storeys * _settings.StoreyHeightMetres;
            float west = plot.Area.Min.X * cellSize;
            float south = plot.Area.Min.Y * cellSize;
            float width = plot.Area.Width * cellSize;
            float depth = plot.Area.Height * cellSize;
            for (int index = 0; index < count; index++)
            {
                float x = west + (width * ((index % columns) + HalfStep) / columns);
                float z = south + (depth * ((index / columns) + HalfStep) / rows);
                GarrisonSlotPose pose = new GarrisonSlotPose(new Vector3(x, roofHeight, z), facing);
                yield return new GarrisonSlot(GarrisonSlotKind.Roof, plot.Storeys, pose);
            }
        }

        private static Vector3 FindEntrance(IReadOnlyList<PlotFacade> roadFacades, CityLayout layout)
        {
            GridPosition cell = roadFacades.SelectMany(facade => facade.AdjacentCells).First(layout.IsRoad);
            return new Vector3((cell.X + HalfStep) * layout.CellSize, 0f, (cell.Y + HalfStep) * layout.CellSize);
        }
    }
}
