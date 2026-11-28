using System;
using System.Linq;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A public park: a lawn behind a stone kerb under plane trees and umbrella pines, benches, wildflowers and, in a
    /// big enough park, a gazebo or a monument in the middle. Units can cut across it. The same plot always gets the
    /// same park.
    /// </summary>
    internal sealed class ParkDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float LawnHeightMetres = 0.06f;
        private const float KerbHeightMetres = 0.15f;
        private const float KerbThicknessMetres = 0.25f;
        private const float TreeSpacingMetres = 7f;
        private const float TreeMarginMetres = 2f;
        private const int PineEvery = 3;
        private const float CentrepieceMinAreaSquareMetres = 300f;
        private const float CentrepieceClearanceMetres = 5f;
        private const int Benches = 2;
        private const float HalfTurnDegrees = 180f;
        private const float BenchOffsetMetres = 3f;
        private const float SquareMetresPerFlowerClump = 40f;
        private static readonly Vector3 GazeboFootprint = new Vector3(4.8f, 1f, 4.8f);
        private static readonly Vector3 MonumentFootprint = new Vector3(2.6f, 1f, 2.6f);

        private readonly LandmarkMaterials _materials;
        private readonly PropModels _models;

        public ParkDressing(LandmarkMaterials materials, PropModels models)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            System.Random random = PlotRandom.For(frame.Plot);
            GroundCover.Lay(props, frame, LawnHeightMetres, _materials.Grass);
            AddKerb(props, frame);
            bool hasCentrepiece = frame.Width * frame.Depth >= CentrepieceMinAreaSquareMetres;
            if (hasCentrepiece)
            {
                AddCentrepiece(props, frame.GroundCentre, random);
            }

            AddTrees(props, frame, random, hasCentrepiece);
            AddBenches(props, frame.GroundCentre, hasCentrepiece);
            int clumps = (int)(frame.Width * frame.Depth / SquareMetresPerFlowerClump);
            foreach (Vector3 spot in PropScatter.Spots(frame, TreeSpacingMetres * Half, 1f, random).Take(clumps))
            {
                props.Model(_models[PropKind.WildflowerClump], spot, PropScatter.AnyHeading(random));
            }
        }

        private void AddCentrepiece(PropFactory props, Vector3 centre, System.Random random)
        {
            bool gazebo = random.Next(2) == 0;
            props.Model(_models[gazebo ? PropKind.Gazebo : PropKind.Monument], centre, PropScatter.SquareHeading(random));
            props.Obstacle(centre, gazebo ? GazeboFootprint : MonumentFootprint);
        }

        private void AddTrees(PropFactory props, PlotFrame frame, System.Random random, bool hasCentrepiece)
        {
            int index = 0;
            foreach (Vector3 spot in PropScatter.Spots(frame, TreeSpacingMetres, TreeMarginMetres, random))
            {
                if (hasCentrepiece && Vector3.Distance(spot, frame.GroundCentre) < CentrepieceClearanceMetres)
                {
                    continue;
                }

                PropKind tree = index % PineEvery == PineEvery - 1 ? PropKind.UmbrellaPine : PropKind.PlaneTree;
                props.Model(_models[tree], spot, PropScatter.AnyHeading(random));
                index++;
            }
        }

        // Benches either side of the middle, facing it.
        private void AddBenches(PropFactory props, Vector3 centre, bool hasCentrepiece)
        {
            float offset = hasCentrepiece ? CentrepieceClearanceMetres - 1f : BenchOffsetMetres;
            for (int index = 0; index < Benches; index++)
            {
                // A bench's front faces +Z unrotated, so the one south of the middle needs no turn.
                float side = index == 0 ? -1f : 1f;
                Quaternion facingCentre = Quaternion.Euler(0f, index == 0 ? 0f : HalfTurnDegrees, 0f);
                props.Model(_models[PropKind.ParkBench], centre + new Vector3(0f, 0f, side * offset), facingCentre);
            }
        }

        private void AddKerb(PropFactory props, PlotFrame frame)
        {
            float y = KerbHeightMetres * Half;
            float east = frame.West + frame.Width;
            float north = frame.South + frame.Depth;
            float midX = frame.West + (frame.Width * Half);
            float midZ = frame.South + (frame.Depth * Half);
            var alongX = new Vector3(frame.Width, KerbHeightMetres, KerbThicknessMetres);
            var alongZ = new Vector3(KerbThicknessMetres, KerbHeightMetres, frame.Depth);
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(midX, y, frame.South), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(midX, y, north), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(frame.West, y, midZ), alongZ, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(east, y, midZ), alongZ, _materials.Stone));
        }
    }
}
