using System;
using System.Linq;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A farmyard or workshop yard: packed gravel with carts, a haystack, woodpiles, crates, barrels and now and then
    /// a boat. All decoration, so units can still cross it. The same plot always gets the same clutter.
    /// </summary>
    internal sealed class YardDressing : IPlotDressing
    {
        private const float GravelHeightMetres = 0.03f;
        private const float SpotSpacingMetres = 4f;
        private const float SpotMarginMetres = 1.5f;
        private const float SquareMetresPerItem = 30f;

        private static readonly PropKind[] Clutter =
        {
            PropKind.FarmCart, PropKind.HayStack, PropKind.WoodPile, PropKind.CrateStack, PropKind.BarrelGroup, PropKind.Handcart,
            PropKind.WoodPile, PropKind.CrateStack, PropKind.RowingBoat,
        };

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public YardDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, GravelHeightMetres, _materials.Stone);
            int count = Math.Max(1, (int)(frame.Width * frame.Depth / SquareMetresPerItem));
            foreach (Vector3 spot in PropScatter.Spots(frame, SpotSpacingMetres, SpotMarginMetres, random).Take(count))
            {
                props.Model(_models[Clutter[random.Next(Clutter.Length)]], spot, PropScatter.AnyHeading(random));
            }
        }
    }
}
