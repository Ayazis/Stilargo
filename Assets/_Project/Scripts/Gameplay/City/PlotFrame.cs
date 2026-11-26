using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>A plot's footprint in world metres, plus what a dressing needs to build on it.</summary>
    internal sealed class PlotFrame
    {
        private const float Half = 0.5f;

        public PlotFrame(Plot plot, float cellSize, float storeyHeight)
        {
            Plot = plot ?? throw new ArgumentNullException(nameof(plot));
            CellSize = cellSize;
            StoreyHeight = storeyHeight;
        }

        public Plot Plot { get; }

        public float CellSize { get; }

        public float StoreyHeight { get; }

        public float West => Plot.Area.Min.X * CellSize;

        public float South => Plot.Area.Min.Y * CellSize;

        public float Width => Plot.Area.Width * CellSize;

        public float Depth => Plot.Area.Height * CellSize;

        public float BuildingHeight => Plot.Storeys * StoreyHeight;

        public Vector3 GroundCentre => new Vector3(West + (Width * Half), 0f, South + (Depth * Half));

        /// <summary>The world rectangle of some cells of this plot, e.g. its bell tower.</summary>
        public Rect WorldRectOf(GridRect area)
        {
            return new Rect(area.Min.X * CellSize, area.Min.Y * CellSize, area.Width * CellSize, area.Height * CellSize);
        }
    }
}
