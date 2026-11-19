using System;
using UnityEngine;

namespace Stilargo.Gameplay.Orders
{
    /// <summary>
    /// Shows the current formation in the top-left corner.
    /// </summary>
    public sealed class FormationLabelView : MonoBehaviour
    {
        private const string LabelFormat = "Formation: {0} (F to switch)";

        [SerializeField] private Rect _area = new Rect(10f, 10f, 300f, 24f);

        private MoveOrderController _controller;

        public void Construct(MoveOrderController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            _controller = controller;
        }

        private void OnGUI()
        {
            if (_controller == null)
            {
                return;
            }

            GUI.Label(_area, string.Format(LabelFormat, _controller.CurrentFormationName));
        }
    }
}
