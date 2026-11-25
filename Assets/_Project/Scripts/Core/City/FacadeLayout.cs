using System;
using System.Collections.Generic;
using System.Numerics;

namespace Stilargo.Core.City
{
    /// <summary>
    /// Lays out facade pieces one bay per cell on every outer wall of a plot: a window per storey,
    /// a door in the middle bay of each ground floor, and a cornice bay along the eaves.
    /// </summary>
    public static class FacadeLayout
    {
        private const float Half = 0.5f;
        private const float FacingNorth = 0f;
        private const float FacingEast = 90f;
        private const float FacingSouth = 180f;
        private const float FacingWest = 270f;

        public static IReadOnlyList<FacadePiece> For(Plot plot, float cellSize, float storeyHeight)
        {
            if (plot == null)
            {
                throw new ArgumentNullException(nameof(plot));
            }

            if (cellSize <= 0f || storeyHeight <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size and storey height must be positive.");
            }

            var pieces = new List<FacadePiece>();
            GridRect area = plot.Area;
            float west = area.Min.X * cellSize;
            float east = (area.Min.X + area.Width) * cellSize;
            float south = area.Min.Y * cellSize;
            float north = (area.Min.Y + area.Height) * cellSize;

            for (int bay = 0; bay < area.Width; bay++)
            {
                float x = (area.Min.X + bay + Half) * cellSize;
                bool isMiddle = IsMiddle(bay, area.Width);
                AddBayColumn(pieces, new BayColumn(new Vector3(x, 0f, south), FacingSouth, isMiddle), plot.Storeys, storeyHeight);
                AddBayColumn(pieces, new BayColumn(new Vector3(x, 0f, north), FacingNorth, isMiddle), plot.Storeys, storeyHeight);
            }

            for (int bay = 0; bay < area.Height; bay++)
            {
                float z = (area.Min.Y + bay + Half) * cellSize;
                bool isMiddle = IsMiddle(bay, area.Height);
                AddBayColumn(pieces, new BayColumn(new Vector3(west, 0f, z), FacingWest, isMiddle), plot.Storeys, storeyHeight);
                AddBayColumn(pieces, new BayColumn(new Vector3(east, 0f, z), FacingEast, isMiddle), plot.Storeys, storeyHeight);
            }

            return pieces;
        }

        private static bool IsMiddle(int bay, int bayCount)
        {
            return bay == bayCount / 2;
        }

        private static void AddBayColumn(List<FacadePiece> pieces, BayColumn column, int storeys, float storeyHeight)
        {
            FacadePieceKind groundFloor = column.IsMiddle ? FacadePieceKind.Door : FacadePieceKind.Window;
            pieces.Add(new FacadePiece(groundFloor, column.Foot, column.Facing));
            for (int storey = 1; storey < storeys; storey++)
            {
                pieces.Add(new FacadePiece(FacadePieceKind.Window, column.AtHeight(storey * storeyHeight), column.Facing));
            }

            pieces.Add(new FacadePiece(FacadePieceKind.Cornice, column.AtHeight(storeys * storeyHeight), column.Facing));
        }

        private readonly struct BayColumn
        {
            public BayColumn(Vector3 foot, float facing, bool isMiddle)
            {
                Foot = foot;
                Facing = facing;
                IsMiddle = isMiddle;
            }

            public Vector3 Foot { get; }

            public float Facing { get; }

            public bool IsMiddle { get; }

            public Vector3 AtHeight(float height)
            {
                return Foot + new Vector3(0f, height, 0f);
            }
        }
    }
}
