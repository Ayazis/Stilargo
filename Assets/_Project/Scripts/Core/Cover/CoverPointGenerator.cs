using System;
using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.City;

namespace Stilargo.Core.Cover
{
    /// <summary>Places cover points on road cells along every edge shared with a building plot.</summary>
    public sealed class CoverPointGenerator
    {
        private const float Half = 0.5f;

        // South, east, north, west: a fixed order keeps generation deterministic.
        private static readonly GridPosition[] NeighbourDirections =
        {
            new GridPosition(0, -1),
            new GridPosition(1, 0),
            new GridPosition(0, 1),
            new GridPosition(-1, 0),
        };

        private readonly CoverSettings _settings;

        public CoverPointGenerator(CoverSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReadOnlyList<CoverPoint> Generate(CityLayout layout)
        {
            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            RequireOffsetFitsCell(layout.CellSize);
            bool[] plotCells = MarkPlotCells(layout);
            List<CoverPoint> points = new List<CoverPoint>();
            foreach (GridPosition road in layout.RoadCells())
            {
                foreach (GridPosition direction in NeighbourDirections)
                {
                    GridPosition neighbour = new GridPosition(road.X + direction.X, road.Y + direction.Y);
                    if (IsPlotCell(layout, plotCells, neighbour))
                    {
                        AddEdgePoints(points, new RoadEdge(road, direction), layout.CellSize);
                    }
                }
            }

            return points;
        }

        private void RequireOffsetFitsCell(float cellSize)
        {
            if (_settings.Placement.WallOffsetMetres >= cellSize * Half)
            {
                throw new InvalidOperationException("Wall offset must be less than half a cell so points stay in their road cell.");
            }
        }

        private static bool[] MarkPlotCells(CityLayout layout)
        {
            bool[] plotCells = new bool[layout.Width * layout.Height];
            foreach (Plot plot in layout.Plots)
            {
                foreach (GridPosition cell in plot.Area.Cells())
                {
                    if (layout.IsInside(cell))
                    {
                        plotCells[(cell.Y * layout.Width) + cell.X] = true;
                    }
                }
            }

            return plotCells;
        }

        private static bool IsPlotCell(CityLayout layout, bool[] plotCells, GridPosition cell)
        {
            if (!layout.IsInside(cell) || layout.IsRoad(cell))
            {
                return false;
            }

            return plotCells[(cell.Y * layout.Width) + cell.X];
        }

        private void AddEdgePoints(List<CoverPoint> points, RoadEdge edge, float cellSize)
        {
            Vector3 toWall = new Vector3(edge.Direction.X, 0f, edge.Direction.Y);
            Vector3 along = new Vector3(Math.Abs(edge.Direction.Y), 0f, Math.Abs(edge.Direction.X));
            Vector3 cellCentre = new Vector3((edge.Road.X + Half) * cellSize, 0f, (edge.Road.Y + Half) * cellSize);
            Vector3 lineCentre = cellCentre + (toWall * ((cellSize * Half) - _settings.Placement.WallOffsetMetres));
            int count = _settings.Placement.PointsPerEdge;
            for (int i = 0; i < count; i++)
            {
                float alongOffset = (((i + Half) / count) - Half) * cellSize;
                points.Add(new CoverPoint(lineCentre + (along * alongOffset), -toWall, _settings.Protection));
            }
        }
    }
}
