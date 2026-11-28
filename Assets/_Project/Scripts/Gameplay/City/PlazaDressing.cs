using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A paved square, after the church-square concept art: plane trees, stone benches, planters, lamps and a café
    /// terrace, with a compass-rose inlay in the middle of a big open square. Units cross the paving freely.
    /// </summary>
    internal sealed class PlazaDressing : IPlotDressing
    {
        private const float PavingHeightMetres = 0.04f;
        private const float InlayMinSizeMetres = 10f;
        private const float InlayClearanceMetres = 4f;
        private const float SpotSpacingMetres = 6f;
        private const float SpotMarginMetres = 1.5f;
        private const float TerraceMinAreaSquareMetres = 80f;

        private static readonly PropKind[] Furniture =
        {
            PropKind.PlaneTree, PropKind.StoneBench, PropKind.Planter, PropKind.StreetLamp, PropKind.PlaneTree, PropKind.StoneBench,
        };

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public PlazaDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, PavingHeightMetres, _materials.Stone);
            bool hasInlay = Mathf.Min(frame.Width, frame.Depth) >= InlayMinSizeMetres;
            if (hasInlay)
            {
                props.Model(_models[PropKind.SquareInlay], frame.GroundCentre + (Vector3.up * PavingHeightMetres), Quaternion.identity);
            }

            var spots = new Queue<Vector3>(PropScatter.Spots(frame, SpotSpacingMetres, SpotMarginMetres, random));
            if (frame.Width * frame.Depth >= TerraceMinAreaSquareMetres && spots.Count > 0)
            {
                props.Model(_models[PropKind.CafeTerrace], spots.Dequeue(), PropScatter.SquareHeading(random));
            }

            int index = 0;
            while (spots.Count > 0)
            {
                Vector3 spot = spots.Dequeue();
                if (hasInlay && Vector3.Distance(spot, frame.GroundCentre) < InlayClearanceMetres)
                {
                    continue;
                }

                props.Model(_models[Furniture[index % Furniture.Length]], spot, PropScatter.SquareHeading(random));
                index++;
            }
        }
    }
}
