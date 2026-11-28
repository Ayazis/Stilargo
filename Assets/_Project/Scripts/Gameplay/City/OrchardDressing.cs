using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>One kind of fruit tree planted in a regular grid on grass, the way an orchard is laid out.</summary>
    internal sealed class OrchardDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float GrassHeightMetres = 0.04f;
        private const float TreeSpacingMetres = 6f;

        private static readonly PropKind[] FruitTrees = { PropKind.FruitTreeA, PropKind.FruitTreeB, PropKind.FruitTreeC };

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public OrchardDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, GrassHeightMetres, _materials.Grass);
            GameObject tree = _models[FruitTrees[random.Next(FruitTrees.Length)]];
            int columns = Math.Max(1, (int)(frame.Width / TreeSpacingMetres));
            int rows = Math.Max(1, (int)(frame.Depth / TreeSpacingMetres));
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    float x = frame.West + (frame.Width * (column + Half) / columns);
                    float z = frame.South + (frame.Depth * (row + Half) / rows);
                    props.Model(tree, new Vector3(x, 0f, z), PropScatter.AnyHeading(random));
                }
            }
        }
    }
}
