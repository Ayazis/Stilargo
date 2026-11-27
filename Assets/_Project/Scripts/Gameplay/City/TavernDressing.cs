using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>A terracotta tavern: an awning and a hanging sign over every door, and barrels stacked beside it.</summary>
    internal sealed class TavernDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float BracketHeightMetres = 2.9f;
        private const float BracketSideMetres = 0.9f;
        private const float AwningHeightMetres = 2.45f;
        private const float BarrelDiameterMetres = 0.6f;
        private const float BarrelHeightMetres = 0.9f;
        private const float BarrelOutMetres = 0.5f;
        private const float BarrelSideMetres = 1.3f;
        private static readonly Vector3 BracketSize = new Vector3(0.06f, 0.06f, 0.9f);
        private static readonly Vector3 SignSize = new Vector3(0.05f, 0.5f, 0.7f);
        private static readonly Vector3 AwningSize = new Vector3(1.6f, 0.06f, 0.8f);

        private readonly LandmarkMaterials _materials;

        public TavernDressing(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
        }

        public bool TryGetWallMaterial(out Material material)
        {
            material = _materials.TavernWall;
            return true;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            foreach (FacadePiece piece in FacadeLayout.For(frame.Plot, frame.CellSize, frame.StoreyHeight))
            {
                if (piece.Kind == FacadePieceKind.Door)
                {
                    DressDoor(props, new Vector3(piece.Position.X, 0f, piece.Position.Z), Quaternion.Euler(0f, piece.FacingDegrees, 0f));
                }
            }
        }

        private void DressDoor(PropFactory props, Vector3 door, Quaternion facing)
        {
            Vector3 outward = facing * Vector3.forward;
            Vector3 side = facing * Vector3.right;

            Vector3 bracketRoot = door + (side * BracketSideMetres) + (Vector3.up * BracketHeightMetres);
            Vector3 bracketEnd = bracketRoot + (outward * BracketSize.z);
            Place(props.Box(bracketRoot + (outward * (BracketSize.z * Half)), BracketSize, _materials.Wood), facing);
            Place(props.Box(bracketEnd - (outward * (SignSize.z * Half)) + (Vector3.down * (SignSize.y * Half)), SignSize, _materials.Wood), facing);
            Place(props.Box(door + (outward * (AwningSize.z * Half)) + (Vector3.up * AwningHeightMetres), AwningSize, _materials.Cloth), facing);

            Vector3 barrels = door + (outward * BarrelOutMetres) - (side * BarrelSideMetres);
            PropFactory.IgnoredByNavigation(props.Cylinder(barrels, BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
            PropFactory.IgnoredByNavigation(props.Cylinder(barrels - (side * BarrelDiameterMetres), BarrelDiameterMetres, BarrelHeightMetres, _materials.Wood));
        }

        private static void Place(GameObject prop, Quaternion facing)
        {
            prop.transform.rotation = facing;
            PropFactory.IgnoredByNavigation(prop);
        }
    }
}
