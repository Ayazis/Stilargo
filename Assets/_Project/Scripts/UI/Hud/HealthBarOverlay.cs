using System;
using System.Collections.Generic;
using Stilargo.Gameplay.Combat;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stilargo.UI.Hud
{
    /// <summary>
    /// A small health bar floating over every unit's head: green for the squad, red for the rival. Bars follow the
    /// camera every frame and keep a fixed size on screen, so they stay readable at any zoom.
    /// </summary>
    internal sealed class HealthBarOverlay : VisualElement
    {
        private const float BarWidthPixels = 44f;
        private const float BarHeightPixels = 6f;
        private const float BorderPixels = 1f;
        private const float PercentPerWhole = 100f;

        private static readonly Color BackgroundColor = new Color(0.05f, 0.05f, 0.06f, 0.75f);
        private static readonly Color FriendlyColor = new Color(0.35f, 0.85f, 0.4f, 1f);
        private static readonly Color HostileColor = new Color(0.92f, 0.26f, 0.2f, 1f);

        private readonly IReadOnlyList<HealthBarSource> _sources;
        private readonly UnityEngine.Camera _camera;
        private readonly List<VisualElement> _bars = new List<VisualElement>();
        private readonly List<VisualElement> _fills = new List<VisualElement>();

        public HealthBarOverlay(IReadOnlyList<HealthBarSource> sources, UnityEngine.Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _sources = sources ?? throw new ArgumentNullException(nameof(sources));
            _camera = camera;
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
        }

        public void Refresh()
        {
            if (panel == null)
            {
                return;
            }

            EnsureBars(_sources.Count);
            for (int index = 0; index < _bars.Count; index++)
            {
                bool isUsed = index < _sources.Count;
                if (!isUsed || !TryPlace(index, _sources[index]))
                {
                    _bars[index].style.display = DisplayStyle.None;
                }
            }
        }

        private bool TryPlace(int index, HealthBarSource source)
        {
            if (!source.IsShown)
            {
                return false;
            }

            Vector3 anchor = source.Anchor;
            if (_camera.WorldToViewportPoint(anchor).z <= 0f)
            {
                return false;
            }

            Vector2 point = RuntimePanelUtils.CameraTransformWorldToPanel(panel, anchor, _camera);
            VisualElement bar = _bars[index];
            bar.style.display = DisplayStyle.Flex;
            bar.style.left = point.x - (BarWidthPixels * 0.5f);
            bar.style.top = point.y - BarHeightPixels;

            VisualElement fill = _fills[index];
            fill.style.width = Length.Percent(Mathf.Clamp01(source.HealthFraction) * PercentPerWhole);
            fill.style.backgroundColor = source.IsHostile ? HostileColor : FriendlyColor;
            return true;
        }

        private void EnsureBars(int count)
        {
            while (_bars.Count < count)
            {
                var bar = new VisualElement { pickingMode = PickingMode.Ignore };
                bar.style.position = Position.Absolute;
                bar.style.width = BarWidthPixels;
                bar.style.height = BarHeightPixels;
                bar.style.backgroundColor = BackgroundColor;
                bar.style.paddingLeft = BorderPixels;
                bar.style.paddingRight = BorderPixels;
                bar.style.paddingTop = BorderPixels;
                bar.style.paddingBottom = BorderPixels;

                var fill = new VisualElement { pickingMode = PickingMode.Ignore };
                fill.style.height = Length.Percent(PercentPerWhole);
                bar.Add(fill);
                Add(bar);
                _bars.Add(bar);
                _fills.Add(fill);
            }
        }
    }
}
