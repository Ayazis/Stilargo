using System;
using Stilargo.Core.City;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Selection
{
    /// <summary>Outlines the selected building's footprint on the street, like the ring under a selected unit.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class BuildingSelectionView : MonoBehaviour
    {
        private const int CornerCount = 4;
        private const float DefaultLiftMetres = 0.08f;
        private const float DefaultMarginMetres = 0.4f;
        private const float DefaultWidthMetres = 0.18f;

        [SerializeField, Min(0f)] private float _liftMetres = DefaultLiftMetres;
        [SerializeField, Min(0f)] private float _marginMetres = DefaultMarginMetres;
        [SerializeField, Min(0.01f)] private float _widthMetres = DefaultWidthMetres;

        private LineRenderer _line;
        private BuildingSelection _selection;
        private float _cellSize;

        public void Construct(BuildingSelection selection, float cellSize, Material material)
        {
            if (_selection != null)
            {
                throw new InvalidOperationException($"{name} is already constructed.");
            }

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            _selection = selection ?? throw new ArgumentNullException(nameof(selection));
            _cellSize = cellSize;
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.loop = true;
            _line.widthMultiplier = _widthMetres;
            _line.sharedMaterial = material;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _selection.Changed += Redraw;
            Redraw();
        }

        private void OnDestroy()
        {
            if (_selection != null)
            {
                _selection.Changed -= Redraw;
            }
        }

        private void Redraw()
        {
            if (!_selection.HasSelection)
            {
                _line.positionCount = 0;
                return;
            }

            GridRect area = _selection.Selected.Plot.Area;
            float west = (area.Min.X * _cellSize) - _marginMetres;
            float south = (area.Min.Y * _cellSize) - _marginMetres;
            float east = (area.XMax * _cellSize) + _marginMetres;
            float north = (area.YMax * _cellSize) + _marginMetres;
            _line.positionCount = CornerCount;
            _line.SetPosition(0, new Vector3(west, _liftMetres, south));
            _line.SetPosition(1, new Vector3(east, _liftMetres, south));
            _line.SetPosition(2, new Vector3(east, _liftMetres, north));
            _line.SetPosition(3, new Vector3(west, _liftMetres, north));
        }
    }
}
