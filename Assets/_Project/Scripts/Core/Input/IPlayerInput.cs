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

        bool WasSelectPressedThisFrame { get; }

        bool IsSelectHeld { get; }

        bool WasSelectReleasedThisFrame { get; }

        bool WasCommandPressedThisFrame { get; }

        /// <summary>True while the modifier that queues orders instead of replacing them is held.</summary>
        bool IsQueueModifierHeld { get; }

        /// <summary>True on the frame the player asks for the next formation.</summary>
        bool WasCycleFormationPressedThisFrame { get; }
    }
}
