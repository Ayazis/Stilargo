using System;
using Stilargo.Core.Hill;
using Stilargo.Core.Match;
using Stilargo.Core.Rival;
using UnityEngine;

namespace Stilargo.Gameplay.Hill
{
    /// <summary>
    /// Placeholder hill HUD in the top-right corner, clear of FL's HUD column on the left: who holds the hill, what
    /// the rival is doing and the final result. FL's HUD shows the hold timer through <see cref="HillStatus"/>.
    /// </summary>
    public sealed class HillHudView : MonoBehaviour
    {
        private const string HillFormat = "Hill: {0}";
        private const string RivalFormat = "Rival: {0}";
        private const string DefeatedRivalText = "wiped out";
        private const string WonText = "You held the hill. You win!";
        private const string LostText = "Your house was destroyed. You lose.";
        private const int ResultFontSize = 22;

        [SerializeField] private float _width = 360f;
        [SerializeField] private float _lineHeight = 24f;
        [SerializeField] private float _margin = 10f;
        [SerializeField] private Vector2 _resultSize = new Vector2(420f, 40f);

        private HillControl _hill;
        private RivalBrain _rival;
        private MatchReferee _referee;
        private GUIStyle _resultStyle;

        public void Construct(HillControl hill, RivalBrain rival, MatchReferee referee)
        {
            if (_hill != null)
            {
                throw new InvalidOperationException($"{nameof(HillHudView)} is already constructed.");
            }

            _hill = hill ?? throw new ArgumentNullException(nameof(hill));
            _rival = rival ?? throw new ArgumentNullException(nameof(rival));
            _referee = referee ?? throw new ArgumentNullException(nameof(referee));
        }

        private void OnGUI()
        {
            if (_hill == null)
            {
                return;
            }

            float left = Screen.width - _width - _margin;
            GUI.Label(new Rect(left, _margin, _width, _lineHeight), HillLine());
            GUI.Label(new Rect(left, _margin + _lineHeight, _width, _lineHeight), string.Format(RivalFormat, RivalLine()));
            if (_referee.IsDecided)
            {
                DrawResult();
            }
        }

        private string HillLine()
        {
            return string.Format(HillFormat, StateText(_hill.State));
        }

        private string RivalLine()
        {
            if (_rival.IsDefeated)
            {
                return DefeatedRivalText;
            }

            return _rival.Current == null ? string.Empty : _rival.Current.Name;
        }

        private void DrawResult()
        {
            if (_resultStyle == null)
            {
                _resultStyle = new GUIStyle(GUI.skin.box) { fontSize = ResultFontSize, alignment = TextAnchor.MiddleCenter };
            }

            var area = new Rect((Screen.width - _resultSize.x) / 2f, (Screen.height - _resultSize.y) / 2f, _resultSize.x, _resultSize.y);
            GUI.Box(area, _referee.Outcome == MatchOutcome.Won ? WonText : LostText, _resultStyle);
        }

        private static string StateText(HillState state)
        {
            switch (state)
            {
                case HillState.HeldByPlayer:
                    return "held by you";
                case HillState.HeldByRival:
                    return "held by the rival";
                case HillState.Contested:
                    return "contested";
                default:
                    return "free";
            }
        }
    }
}
