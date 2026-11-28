using System;
using Stilargo.Core.Fog;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>
    /// Paints the visibility grid into a texture that a full-screen pass lays over the rendered frame. Unseen cells
    /// are black (nothing shows, not even the map), explored cells out of view are in shadow (the map shows dimmed,
    /// units there are hidden by <see cref="FogHideable"/>), visible cells are clear.
    /// </summary>
    public sealed class FogOverlay : MonoBehaviour
    {
        private static readonly int FogMapId = Shader.PropertyToID("_FogMap");
        private static readonly int FogMapWorldSizeId = Shader.PropertyToID("_FogMapWorldSize");
        private static readonly int OutsideColorId = Shader.PropertyToID("_OutsideColor");

        // Near-black rather than pure black, so the edge of the explored map reads as night, not a hole.
        [SerializeField] private Color32 _unseenColor = new Color32(5, 5, 6, 255);
        [SerializeField] private Color32 _shadowColor = new Color32(8, 8, 12, 165);
        [SerializeField] private Color32 _visibleColor = new Color32(8, 8, 12, 0);
        [SerializeField] private Color32 _outsideColor = new Color32(5, 5, 6, 255);
        [SerializeField] private Material _screenMaterial;

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

            if (_screenMaterial == null)
            {
                throw new InvalidOperationException($"{nameof(FogOverlay)} needs its screen material assigned.");
            }

            _grid = grid;
            CreateTexture();
            CreateScreenPass();
            if (isActiveAndEnabled)
            {
                _grid.Refreshed += Repaint;
            }

            Repaint();
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

        private void CreateTexture()
        {
            _texture = new Texture2D(_grid.Width, _grid.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _pixels = new Color32[_grid.Width * _grid.Height];
        }

        // After post processing, so auto exposure and tonemapping can't lift the black back to grey.
        private void CreateScreenPass()
        {
            _material = new Material(_screenMaterial);
            _material.SetTexture(FogMapId, _texture);
            _material.SetVector(FogMapWorldSizeId, new Vector4(_grid.Width * _grid.CellSize, _grid.Height * _grid.CellSize, 0f, 0f));

            CustomPassVolume volume = gameObject.AddComponent<CustomPassVolume>();
            volume.isGlobal = true;
            volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
            volume.customPasses.Add(new FullScreenCustomPass
            {
                name = "Fog of war",
                fullscreenPassMaterial = _material,
                fetchColorBuffer = false
            });
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
            // Beyond the map is darkness only while the fog is on; with it off, anything placed out there must show too.
            _material.SetColor(OutsideColorId, _grid.IsFogEnabled ? (Color)_outsideColor : _visibleColor);
        }

        private Color32 ColorFor(VisibilityState state)
        {
            switch (state)
            {
                case VisibilityState.Visible:
                    return _visibleColor;
                case VisibilityState.Explored:
                    return _shadowColor;
                default:
                    return _unseenColor;
            }
        }
    }
}
