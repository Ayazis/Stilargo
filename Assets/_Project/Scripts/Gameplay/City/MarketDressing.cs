using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The market square, after the market-and-chapel concept art: open paving with a timber market hall in the
    /// middle where it fits, rows of striped stalls along two sides facing inwards, and a tree in each corner.
    /// Units walk through the hall; only the stalls are in the way.
    /// </summary>
    internal sealed class MarketDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float PavingHeightMetres = 0.04f;
        private const float HallMinWidthMetres = 14f;
        private const float HallMinDepthMetres = 14f;
        private const float StallInsetMetres = 2.2f;
        private const float StallSpacingMetres = 4f;
        private const float StallRowFraction = 0.7f;
        private const float TreeInsetMetres = 2f;
        private const float HalfTurnDegrees = 180f;
        private static readonly Vector3 StallFootprint = new Vector3(3.2f, 1f, 2.2f);

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public MarketDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            GroundCover.Lay(props, frame, PavingHeightMetres, _materials.Stone);
            if (frame.Width >= HallMinWidthMetres && frame.Depth >= HallMinDepthMetres)
            {
                props.Model(_models[PropKind.MarketHall], frame.GroundCentre, Quaternion.identity);
            }

            // A stall's counter faces +Z unrotated: the south row faces north into the square, the north row south.
            AddStallRow(props, frame, frame.South + StallInsetMetres, Quaternion.identity);
            AddStallRow(props, frame, frame.South + frame.Depth - StallInsetMetres, Quaternion.Euler(0f, HalfTurnDegrees, 0f));
            AddCornerTrees(props, frame);
        }

        private void AddStallRow(PropFactory props, PlotFrame frame, float z, Quaternion facing)
        {
            int stalls = Math.Max(1, (int)(frame.Width * StallRowFraction / StallSpacingMetres));
            float firstX = frame.GroundCentre.x - ((stalls - 1) * StallSpacingMetres * Half);
            for (int index = 0; index < stalls; index++)
            {
                var foot = new Vector3(firstX + (index * StallSpacingMetres), 0f, z);
                props.Model(_models[PropKind.MarketStall], foot, facing);
                props.Obstacle(foot, StallFootprint);
            }
        }

        private void AddCornerTrees(PropFactory props, PlotFrame frame)
        {
            float west = frame.West + TreeInsetMetres;
            float east = frame.West + frame.Width - TreeInsetMetres;
            float south = frame.South + TreeInsetMetres;
            float north = frame.South + frame.Depth - TreeInsetMetres;
            foreach (Vector3 corner in new[] { new Vector3(west, 0f, south), new Vector3(east, 0f, south), new Vector3(west, 0f, north), new Vector3(east, 0f, north) })
            {
                props.Model(_models[PropKind.PlaneTree], corner, Quaternion.identity);
            }
        }
    }
}
