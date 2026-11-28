using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A private garden between villas or in a house block's courtyard: lawn inside a low stone wall, fruit trees,
    /// shrubs, a vegetable bed, a washing line and a bench. The same plot always gets the same garden.
    /// </summary>
    internal sealed class GardenDressing : IPlotDressing
    {
        private const float LawnHeightMetres = 0.05f;
        private const float WallSegmentMetres = 2f;
        private const float SpotSpacingMetres = 3.5f;
        private const float SpotMarginMetres = 1.5f;
        private const float SquareMetresPerTree = 40f;
        private const float BedMinAreaSquareMetres = 30f;
        private const float LineMinAreaSquareMetres = 50f;
        private const int Shrubs = 2;

        private static readonly PropKind[] FruitTrees = { PropKind.FruitTreeA, PropKind.FruitTreeB, PropKind.FruitTreeC };

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public GardenDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, LawnHeightMetres, _materials.Grass);
            PlotEdges.Line(props, frame, _models[PropKind.GardenWall], WallSegmentMetres);

            var spots = new Queue<Vector3>(PropScatter.Spots(frame, SpotSpacingMetres, SpotMarginMetres, random));
            foreach (PropKind kind in Contents(frame.Width * frame.Depth, random))
            {
                if (spots.Count == 0)
                {
                    return;
                }

                props.Model(_models[kind], spots.Dequeue(), PropScatter.SquareHeading(random));
            }
        }

        private static IEnumerable<PropKind> Contents(float area, System.Random random)
        {
            int trees = Math.Max(1, (int)(area / SquareMetresPerTree));
            for (int index = 0; index < trees; index++)
            {
                yield return FruitTrees[random.Next(FruitTrees.Length)];
            }

            if (area >= BedMinAreaSquareMetres)
            {
                yield return PropKind.VegetableBed;
            }

            if (area >= LineMinAreaSquareMetres)
            {
                yield return PropKind.WashingLine;
            }

            yield return PropKind.ParkBench;
            for (int index = 0; index < Shrubs; index++)
            {
                yield return PropKind.Shrub;
            }
        }
    }
}
