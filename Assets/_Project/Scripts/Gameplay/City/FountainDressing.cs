using System;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The centre piece of a square, park or courtyard: the octagonal fountain where there is room for it, otherwise
    /// a courtyard well. Units walk round either.
    /// </summary>
    internal sealed class FountainDressing : IPlotDressing
    {
        private const float FountainMinSizeMetres = 4f;
        private static readonly Vector3 FountainFootprint = new Vector3(4.2f, 1f, 4.2f);
        private static readonly Vector3 WellFootprint = new Vector3(1.8f, 1f, 1.8f);

        private readonly PropModels _models;

        public FountainDressing(PropModels models)
        {
            _models = models ?? throw new ArgumentNullException(nameof(models));
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            bool fits = Mathf.Min(frame.Width, frame.Depth) >= FountainMinSizeMetres;
            Vector3 centre = frame.GroundCentre;
            props.Model(_models[fits ? PropKind.Fountain : PropKind.Well], centre, PropScatter.SquareHeading(PlotRandom.For(frame.Plot)));
            props.Obstacle(centre, fits ? FountainFootprint : WellFootprint);
        }
    }
}
