using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Builds the street life the Core planner placed: lamps, street trees, crates, barrels, planters and handcarts,
    /// each pushed against the wall or kerb beside its pavement cell. All of it is decoration, so it never blocks
    /// a unit or a click.
    /// </summary>
    internal sealed class StreetPropBuilder
    {
        private const float Half = 0.5f;
        private const float WallGapMetres = 0.6f;
        private const float QuarterTurnDegrees = 90f;

        private readonly PropModels _models;
        private readonly Dictionary<StreetPropKind, Func<GridPosition, PropKind>> _kinds;

        public StreetPropBuilder(PropModels models)
        {
            _models = models ?? throw new ArgumentNullException(nameof(models));
            _kinds = new Dictionary<StreetPropKind, Func<GridPosition, PropKind>>
            {
                { StreetPropKind.Lamp, _ => PropKind.StreetLamp },
                { StreetPropKind.Tree, StreetTree },
                { StreetPropKind.Crates, _ => PropKind.CrateStack },
                { StreetPropKind.Barrels, _ => PropKind.BarrelGroup },
                { StreetPropKind.Planter, _ => PropKind.StreetPlanter },
                { StreetPropKind.Cart, _ => PropKind.Handcart },
            };
        }

        public void Build(StreetProp prop, float cellSize, PropFactory props)
        {
            if (!_kinds.TryGetValue(prop.Kind, out Func<GridPosition, PropKind> kindAt))
            {
                throw new InvalidOperationException($"No street prop look for {prop.Kind}.");
            }

            var towards = new Vector3(prop.Towards.X, 0f, prop.Towards.Y);
            var centre = new Vector3((prop.Cell.X + Half) * cellSize, 0f, (prop.Cell.Y + Half) * cellSize);
            Vector3 foot = centre + (towards * ((cellSize * Half) - WallGapMetres));

            // Long props run along the wall, which lies across the direction towards the plot.
            Quaternion alongWall = prop.Towards.X != 0 ? Quaternion.Euler(0f, QuarterTurnDegrees, 0f) : Quaternion.identity;
            props.Model(_models[kindAt(prop.Cell)], foot, alongWall);
        }

        // Plane trees and umbrella pines alternate down a street.
        private static PropKind StreetTree(GridPosition cell)
        {
            return (cell.X + cell.Y) % 2 == 0 ? PropKind.PlaneTree : PropKind.UmbrellaPine;
        }
    }
}
