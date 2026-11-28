using System;
using System.Linq;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>Open grassland in the countryside: wildflowers, rocks and a few grazing sheep on a lawn units walk over.</summary>
    internal sealed class MeadowDressing : IPlotDressing
    {
        private const float GrassHeightMetres = 0.04f;
        private const float SpotSpacingMetres = 5f;
        private const float SpotMarginMetres = 1f;
        private const float SquareMetresPerItem = 45f;

        private static readonly PropKind[] Scatter =
        {
            PropKind.WildflowerClump, PropKind.WildflowerClump, PropKind.WildflowerClump, PropKind.RockA, PropKind.RockB, PropKind.Sheep,
            PropKind.Sheep,
        };

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public MeadowDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, GrassHeightMetres, _materials.Grass);
            int count = Math.Max(1, (int)(frame.Width * frame.Depth / SquareMetresPerItem));
            foreach (Vector3 spot in PropScatter.Spots(frame, SpotSpacingMetres, SpotMarginMetres, random).Take(count))
            {
                props.Model(_models[Scatter[random.Next(Scatter.Length)]], spot, PropScatter.AnyHeading(random));
            }
        }
    }
}
