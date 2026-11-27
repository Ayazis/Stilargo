using System;
using Stilargo.Core.Fog;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>
    /// Paints the visibility grid into a texture that a decal projects straight down onto the map, so the fog
    /// darkens the ground and buildings themselves instead of floating above them as a sheet.
    /// </summary>
    [RequireComponent(typeof(DecalProjector))]
    public sealed class FogOverlay : MonoBehaviour
    {
        private const float Half = 0.5f;
        private const float FlatRotationX = 90f;

        // Dark earth tones rather than pure black, so fogged areas read as shadowed ground, not holes.
        [SerializeField] private Color32 _unseenColor = new Color32(14, 12, 10, 235);
        [SerializeField] private Color32 _exploredColor = new Color32(14, 12, 10, 150);
        [SerializeField] private Color32 _visibleColor = new Color32(14, 12, 10, 0);
        [SerializeField] private string _textureProperty = "_BaseColorMap";

        // Tall enough to reach from the ground over the highest roofs.
        [SerializeField, Min(1f)] private float _projectionDepthMetres = 80f;

        private DecalProjector _projector;
        private Material _material;
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
            if (_material != null)
            {
                Destroy(_material);
            }

            if (_texture != null)
            {
                Destroy(_texture);
            }
        }

        // Construct can run before Awake when the bootstrapper wires scene objects, so both paths cache.
        private void CacheComponents()
        {
            if (_projector == null)
            {
                _projector = GetComponent<DecalProjector>();
            }
        }

        // Rotating 90 degrees on X points the projector down and its V axis along world Z, so the decal UV
        // origin lands on world (0, 0), matching VisibilityGrid's cell layout. The box spans ground to roofs.
        private void FitToGrid()
        {
            float width = _grid.Width * _grid.CellSize;
            float depth = _grid.Height * _grid.CellSize;
            transform.SetPositionAndRotation(
                new Vector3(width * Half, _projectionDepthMetres * Half, depth * Half),
                Quaternion.Euler(FlatRotationX, 0f, 0f));
            _projector.pivot = Vector3.zero;
            _projector.size = new Vector3(width, depth, _projectionDepthMetres);
        }

        private void CreateTexture()
        {
            _texture = new Texture2D(_grid.Width, _grid.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[_grid.Width * _grid.Height];
            _material = new Material(_projector.material);
            _material.SetTexture(_textureProperty, _texture);
            _projector.material = _material;
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
