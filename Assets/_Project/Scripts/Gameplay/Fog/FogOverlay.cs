using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>Paints the visibility grid into a texture on this object's renderer (a quad above the map).</summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class FogOverlay : MonoBehaviour
    {
        private const float Half = 0.5f;
        private const float FlatRotationX = 90f;

        [SerializeField] private Color32 _unseenColor = new Color32(0, 0, 0, 255);
        [SerializeField] private Color32 _exploredColor = new Color32(0, 0, 0, 160);
        [SerializeField] private Color32 _visibleColor = new Color32(0, 0, 0, 0);
        [SerializeField] private string _textureProperty = "_BaseMap";
        [SerializeField] private float _heightAboveGround = 20f;

        private Renderer _renderer;
        private VisibilityGrid _grid;
        private Texture2D _texture;
        private Color32[] _pixels;

        public void Construct(VisibilityGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (_grid != null)
            {
                throw new InvalidOperationException($"{nameof(FogOverlay)} is already constructed.");
            }

            _grid = grid;
            CacheComponents();
            FitToGrid();
            CreateTexture();
            if (isActiveAndEnabled)
            {
                _grid.Refreshed += Repaint;
            }

            Repaint();
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Refreshed += Repaint;
            Repaint();
        }

        private void OnDisable()
        {
            if (_grid == null)
            {
                return;
            }

            _grid.Refreshed -= Repaint;
        }

        private void OnDestroy()
        {
            if (_texture == null)
            {
                return;
            }

            Destroy(_texture);
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }
        }

        // Assumes a Unity quad: lying flat (rotated 90 degrees on X) puts its UV origin at world (0, 0)
        // and its V axis along world Z, matching VisibilityGrid's cell layout.
        private void FitToGrid()
        {
            float width = _grid.Width * _grid.CellSize;
            float depth = _grid.Height * _grid.CellSize;
            transform.SetPositionAndRotation(
                new Vector3(width * Half, _heightAboveGround, depth * Half),
                Quaternion.Euler(FlatRotationX, 0f, 0f));
            transform.localScale = new Vector3(width, depth, 1f);
        }

        private void CreateTexture()
        {
            _texture = new Texture2D(_grid.Width, _grid.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[_grid.Width * _grid.Height];
            _renderer.material.SetTexture(_textureProperty, _texture);
        }

        private void Repaint()
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    _pixels[(y * _grid.Width) + x] = ColorFor(_grid.GetState(x, y));
                }
            }

            _texture.SetPixels32(_pixels);
            _texture.Apply();
        }

        private Color32 ColorFor(VisibilityState state)
        {
            switch (state)
            {
                case VisibilityState.Visible:
                    return _visibleColor;
                case VisibilityState.Explored:
                    return _exploredColor;
                default:
                    return _unseenColor;
            }
        }
    }
}
