using System;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Selection
{
    /// <summary>
    /// Draws the selection drag rectangle on screen while the player is box-selecting.
    /// </summary>
    public sealed class SelectionBoxView : MonoBehaviour
    {
        private const float DefaultBorderWidth = 2f;

        [SerializeField] private Color _fillColor = new Color(0.3f, 0.8f, 1f, 0.15f);
        [SerializeField] private Color _borderColor = new Color(0.3f, 0.8f, 1f, 0.9f);
        [SerializeField] private float _borderWidth = DefaultBorderWidth;

        private SelectionDrag _drag;

        public void Construct(SelectionDrag drag)
        {
            _drag = drag ?? throw new ArgumentNullException(nameof(drag));
        }

        private void OnGUI()
        {
            if (_drag == null || !_drag.IsActive || !_drag.IsBox)
            {
                return;
            }

            Rect rect = ToGuiRect(_drag.Rect);
            DrawRect(rect, _fillColor);
            DrawBorder(rect);
        }

        /// <summary>GUI has its origin top-left while the drag uses bottom-left, so Y is flipped.</summary>
        private static Rect ToGuiRect(ScreenRect screenRect)
        {
            float top = Screen.height - screenRect.Max.Y;
            return new Rect(screenRect.Min.X, top, screenRect.Width, screenRect.Height);
        }

        private void DrawBorder(Rect rect)
        {
            DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, _borderWidth), _borderColor);
            DrawRect(new Rect(rect.xMin, rect.yMax - _borderWidth, rect.width, _borderWidth), _borderColor);
            DrawRect(new Rect(rect.xMin, rect.yMin, _borderWidth, rect.height), _borderColor);
            DrawRect(new Rect(rect.xMax - _borderWidth, rect.yMin, _borderWidth, rect.height), _borderColor);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
