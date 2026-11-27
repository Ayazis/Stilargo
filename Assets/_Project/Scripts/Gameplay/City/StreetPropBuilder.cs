using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Builds the street life the Core planner placed: lamps, street trees, crates, barrels, planters and carts,
    /// each pushed against the wall or kerb beside its pavement cell. All of it is decoration, so it never blocks
    /// a unit or a click.
    /// </summary>
    internal sealed class StreetPropBuilder
    {
        private const float Half = 0.5f;
        private const float WallGapMetres = 0.45f;
        private const float QuarterTurnDegrees = 90f;
        private const float LampPoleDiameterMetres = 0.12f;
        private const float LampPoleHeightMetres = 3.4f;
        private const float BarrelDiameterMetres = 0.6f;
        private const float BarrelHeightMetres = 0.9f;
        private const float BarrelSpacingMetres = 0.65f;
        private const float CrateEdgeMetres = 0.6f;
        private const float StackedCrateCentreInEdges = 1.5f;
        private const float WheelDiameterMetres = 0.9f;
        private const float WheelWidthMetres = 0.08f;
        private const float CartBedHeightMetres = 0.6f;
        private static readonly Vector3 LanternSize = new Vector3(0.3f, 0.4f, 0.3f);
        private static readonly Vector3 PlanterSize = new Vector3(0.9f, 0.5f, 0.5f);
        private static readonly Vector3 PlanterShrubSize = new Vector3(0.9f, 0.7f, 0.6f);
        private static readonly Vector3 CartBedSize = new Vector3(1.8f, 0.35f, 1f);

        private readonly LandmarkMaterials _materials;
        private readonly TreeProps _trees;
        private readonly Dictionary<StreetPropKind, Action<PropFactory, Vector3, Quaternion>> _builders;

        public StreetPropBuilder(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _trees = new TreeProps(materials);
            _builders = new Dictionary<StreetPropKind, Action<PropFactory, Vector3, Quaternion>>
            {
                { StreetPropKind.Lamp, (props, foot, _) => AddLamp(props, foot) },
                { StreetPropKind.Tree, (props, foot, _) => AddTree(props, foot) },
                { StreetPropKind.Crates, AddCrates },
                { StreetPropKind.Barrels, AddBarrels },
                { StreetPropKind.Planter, AddPlanter },
                { StreetPropKind.Cart, AddCart },
            };
        }

        public void Build(StreetProp prop, float cellSize, PropFactory props)
        {
            if (!_builders.TryGetValue(prop.Kind, out Action<PropFactory, Vector3, Quaternion> build))
            {
                throw new InvalidOperationException($"No street prop look for {prop.Kind}.");
            }

            var towards = new Vector3(prop.Towards.X, 0f, prop.Towards.Y);
            Vector3 centre = new Vector3((prop.Cell.X + Half) * cellSize, 0f, (prop.Cell.Y + Half) * cellSize);
            Vector3 foot = centre + (towards * ((cellSize * Half) - WallGapMetres));

            // Long props run along the wall, which lies across the direction towards the plot.
            Quaternion alongWall = prop.Towards.X != 0 ? Quaternion.Euler(0f, QuarterTurnDegrees, 0f) : Quaternion.identity;
            build(props, foot, alongWall);
        }

        private void AddLamp(PropFactory props, Vector3 foot)
        {
            PropFactory.Decorative(props.Cylinder(foot, LampPoleDiameterMetres, LampPoleHeightMetres, _materials.Shadow));
            PropFactory.Decorative(props.Box(foot + (Vector3.up * (LampPoleHeightMetres + (LanternSize.y * Half))), LanternSize, _materials.Cloth));
        }

        private void AddTree(PropFactory props, Vector3 foot)
        {
            _trees.AddPine(props, foot);
        }

        private void AddCrates(PropFactory props, Vector3 foot, Quaternion alongWall)
        {
            var size = Vector3.one * CrateEdgeMetres;
            Vector3 side = alongWall * Vector3.right * CrateEdgeMetres;
            PropFactory.Decorative(props.Box(foot + (Vector3.up * (CrateEdgeMetres * Half)), size, _materials.Wood));
            PropFactory.Decorative(props.Box(foot + side + (Vector3.up * (CrateEdgeMetres * Half)), size, _materials.Wood));
            PropFactory.Decorative(props.Box(foot + (side * Half) + (Vector3.up * (CrateEdgeMetres * StackedCrateCentreInEdges)), size, _materials.Wood));
        }

        private void AddBarrels(PropFactory props, Vector3 foot, Quaternion alongWall)
        {
            Vector3 side = alongWall * Vector3.right * BarrelSpacingMetres;
            PropFactory.Decorative(props.Cylinder(foot - side, BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
            PropFactory.Decorative(props.Cylinder(foot, BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
            PropFactory.Decorative(props.Cylinder(foot + side, BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
        }

        private void AddPlanter(PropFactory props, Vector3 foot, Quaternion alongWall)
        {
            GameObject box = PropFactory.Decorative(props.Box(foot + (Vector3.up * (PlanterSize.y * Half)), PlanterSize, _materials.Stone));
            box.transform.rotation = alongWall;
            GameObject shrub = PropFactory.Decorative(props.Sphere(foot + (Vector3.up * PlanterSize.y), PlanterShrubSize, _materials.Foliage));
            shrub.transform.rotation = alongWall;
        }

        private void AddCart(PropFactory props, Vector3 foot, Quaternion alongWall)
        {
            Vector3 bedCentre = foot + (Vector3.up * CartBedHeightMetres);
            GameObject bed = PropFactory.Decorative(props.Box(bedCentre, CartBedSize, _materials.Wood));
            bed.transform.rotation = alongWall;

            Vector3 axle = alongWall * Vector3.forward * ((CartBedSize.z * Half) + WheelWidthMetres);
            Quaternion wheelTurn = alongWall * Quaternion.Euler(QuarterTurnDegrees, 0f, 0f);
            foreach (Vector3 hub in new[] { foot + axle, foot - axle })
            {
                Vector3 wheelBase = hub + (Vector3.up * (WheelDiameterMetres * Half));
                GameObject wheel = PropFactory.Decorative(props.Cylinder(wheelBase, WheelDiameterMetres, WheelWidthMetres, _materials.Wood));
                wheel.transform.position = wheelBase;
                wheel.transform.rotation = wheelTurn;
            }
        }
    }
}
