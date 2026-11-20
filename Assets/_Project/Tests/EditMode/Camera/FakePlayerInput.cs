using System.Numerics;
using Stilargo.Core.Input;

namespace Stilargo.Tests.EditMode.Camera
{
    /// <summary>
    /// Input whose values each test sets directly.
    /// </summary>
    public sealed class FakePlayerInput : IPlayerInput
    {
        public Vector2 PanAxis { get; set; }

        public float ZoomDelta { get; set; }

        public Vector2 PointerScreenPosition { get; set; }

        public bool WasSelectPressedThisFrame { get; set; }

        public bool IsSelectHeld { get; set; }

        public bool WasSelectReleasedThisFrame { get; set; }

        public bool WasCommandPressedThisFrame { get; set; }

        public bool IsQueueModifierHeld { get; set; }

        public bool WasCycleFormationPressedThisFrame { get; set; }

        public bool WasAggressiveStancePressedThisFrame { get; set; }

        public bool WasDefensiveStancePressedThisFrame { get; set; }

        public bool WasHoldStancePressedThisFrame { get; set; }
    }
}
