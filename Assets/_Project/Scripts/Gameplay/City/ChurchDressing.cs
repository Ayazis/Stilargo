using System;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// A stone church with a bell tower at one corner. The tower has a flat top behind a parapet, because soldiers
    /// garrison up there to look out over the city.
    /// </summary>
    internal sealed class ChurchDressing : IPlotDressing
    {
        private const float Half = 0.5f;
        private const float ParapetHeightMetres = 0.7f;
        private const float ParapetThicknessMetres = 0.3f;
        private const float BelfryHeightMetres = 1.8f;
        private const float BelfryWidthShare = 0.45f;
        private const float BelfryDepthMetres = 0.1f;
        private const float BelfryBelowTopMetres = 0.6f;

        private readonly LandmarkMaterials _materials;

        public ChurchDressing(LandmarkMaterials materials)
        {
            _materials = materials ?? throw new ArgumentNullException(nameof(materials));
        }

        public bool TryGetWallMaterial(out Material material)
        {
            material = _materials.Stone;
            return true;
        }

        public void Dress(PlotFrame frame, PropFactory props)
        {
            if (!frame.Plot.HasTower)
            {
                return;
            }

            Rect tower = frame.WorldRectOf(BellTower.AreaOf(frame.Plot));
            float height = frame.Plot.TowerStoreys * frame.StoreyHeight;
            props.Box(new Vector3(tower.center.x, height * Half, tower.center.y), new Vector3(tower.width, height, tower.height), _materials.Stone);
            AddBelfry(props, tower, height);
            AddParapet(props, tower, height);
        }

        private void AddBelfry(PropFactory props, Rect tower, float height)
        {
            float y = height - BelfryBelowTopMetres - (BelfryHeightMetres * Half);
            var acrossX = new Vector3(tower.width * BelfryWidthShare, BelfryHeightMetres, BelfryDepthMetres);
            var acrossZ = new Vector3(BelfryDepthMetres, BelfryHeightMetres, tower.height * BelfryWidthShare);
            props.Box(new Vector3(tower.center.x, y, tower.yMin), acrossX, _materials.Shadow);
            props.Box(new Vector3(tower.center.x, y, tower.yMax), acrossX, _materials.Shadow);
            props.Box(new Vector3(tower.xMin, y, tower.center.y), acrossZ, _materials.Shadow);
            props.Box(new Vector3(tower.xMax, y, tower.center.y), acrossZ, _materials.Shadow);
        }

        private void AddParapet(PropFactory props, Rect tower, float height)
        {
            float y = height + (ParapetHeightMetres * Half);
            var alongX = new Vector3(tower.width, ParapetHeightMetres, ParapetThicknessMetres);
            var alongZ = new Vector3(ParapetThicknessMetres, ParapetHeightMetres, tower.height);
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(tower.center.x, y, tower.yMin), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(tower.center.x, y, tower.yMax), alongX, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(tower.xMin, y, tower.center.y), alongZ, _materials.Stone));
            PropFactory.IgnoredByNavigation(props.Box(new Vector3(tower.xMax, y, tower.center.y), alongZ, _materials.Stone));
        }
    }
}
