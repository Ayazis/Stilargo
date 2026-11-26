using System.Numerics;

namespace Stilargo.Core.Input
{
    /// <summary>
    /// The player's input for the current frame, independent of any input backend.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>Keyboard pan direction, each axis in [-1, 1]. X is right, Y is forward.</summary>
        Vector2 PanAxis { get; }

        /// <summary>Scroll this frame. Positive zooms in.</summary>
        float ZoomDelta { get; }

        /// <summary>Pointer position in screen pixels, origin bottom-left.</summary>
        Vector2 PointerScreenPosition { get; }

        /// <summary>Pointer movement this frame in screen pixels.</summary>
        Vector2 PointerDelta { get; }

        /// <summary>Camera turn keys in [-1, 1]. Positive turns the view clockwise seen from above.</summary>
        float TurnAxis { get; }

        /// <summary>True while the button that turns and tilts the camera by dragging is held.</summary>
        bool IsOrbitDragHeld { get; }

        /// <summary>False when the chosen camera preset turns off panning at the screen edge.</summary>
        bool IsEdgeScrollEnabled { get; }

        bool WasSelectPressedThisFrame { get; }

        bool IsSelectHeld { get; }

        bool WasSelectReleasedThisFrame { get; }

        bool WasCommandPressedThisFrame { get; }

        /// <summary>True while the modifier that queues orders instead of replacing them is held.</summary>
        bool IsQueueModifierHeld { get; }

        /// <summary>True on the frame the player asks for the next formation.</summary>
        bool WasCycleFormationPressedThisFrame { get; }

        /// <summary>True on the frame the player asks for the Aggressive stance.</summary>
        bool WasAggressiveStancePressedThisFrame { get; }

        /// <summary>True on the frame the player asks for the Defensive stance.</summary>
        bool WasDefensiveStancePressedThisFrame { get; }

        /// <summary>True on the frame the player asks for the Hold stance.</summary>
        bool WasHoldStancePressedThisFrame { get; }

        /// <summary>True on the frame the player asks to open or close the pause menu.</summary>
        bool WasPauseMenuPressedThisFrame { get; }
    }
}
