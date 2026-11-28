using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A field of wheat or vegetables: crop patches tiled over bare earth, their rows running along the parcel's
    /// longer side. Low enough to walk and see across. The same plot always grows the same crop.
    /// </summary>
    internal sealed class FieldDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float EarthHeightMetres = 0.04f;

        // The Blender patch is 4 m square; it is stretched to about twice that so a field needs a quarter of the tiles.
        private const float PatchModelMetres = 4f;
        private const float PatchTargetMetres = 8f;
        private const float QuarterTurnDegrees = 90f;
        private const int WheatChanceInThree = 2;
        private const int Three = 3;

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public FieldDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, EarthHeightMetres, _materials.Wood);
            GameObject patch = _models[random.Next(Three) < WheatChanceInThree ? PropKind.WheatPatch : PropKind.VegetableRows];

            // The patch's rows run along its local X; turn it when the field is deeper than wide.
            bool rowsAlongX = frame.Width >= frame.Depth;
            Quaternion rotation = rowsAlongX ? Quaternion.identity : Quaternion.Euler(0f, QuarterTurnDegrees, 0f);
            int columns = Math.Max(1, Mathf.RoundToInt(frame.Width / PatchTargetMetres));
            int rows = Math.Max(1, Mathf.RoundToInt(frame.Depth / PatchTargetMetres));
            float tileX = frame.Width / columns;
            float tileZ = frame.Depth / rows;
            Vector3 scale = rowsAlongX
                ? new Vector3(tileX / PatchModelMetres, 1f, tileZ / PatchModelMetres)
                : new Vector3(tileZ / PatchModelMetres, 1f, tileX / PatchModelMetres);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    var foot = new Vector3(frame.West + (tileX * (column + Half)), EarthHeightMetres, frame.South + (tileZ * (row + Half)));
                    props.Model(patch, foot, rotation).transform.localScale = scale;
                }
            }
        }
    }
}
