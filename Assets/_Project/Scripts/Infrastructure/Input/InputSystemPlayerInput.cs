using System;
using Stilargo.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using NumericsVector2 = System.Numerics.Vector2;
using UnityVector2 = UnityEngine.Vector2;

namespace Stilargo.Infrastructure.Input
{
    /// <summary>
    /// <see cref="IPlayerInput"/> backed by the Input System. The actions are built in code
    /// rather than from an .inputactions asset so there is no editor-generated wrapper to keep in sync.
    /// </summary>
    /// <remarks>
    /// A plain C# class instead of a MonoBehaviour: it has no scene presence of its own, so the
    /// composition root owns its lifetime explicitly through Enable, Disable and Dispose.
    /// </remarks>
    public sealed class InputSystemPlayerInput : IPlayerInput, IDisposable
    {
        private const string ActionMapName = "Player";
        private const string PanActionName = "Pan";
        private const string ZoomActionName = "Zoom";
        private const string PointerActionName = "Pointer";
        private const string SelectActionName = "Select";
        private const string CommandActionName = "Command";
        private const string QueueModifierActionName = "QueueModifier";
        private const string CycleFormationActionName = "CycleFormation";

        private const string Vector2ControlType = "Vector2";
        private const string AxisControlType = "Axis";

        private const float UniformScrollUnitsPerNotch = 1f;
        private const float WindowsRawScrollUnitsPerNotch = 120f;

        private readonly InputActionMap _actionMap;
        private readonly InputAction _pan;
        private readonly InputAction _zoom;
        private readonly InputAction _pointer;
        private readonly InputAction _select;
        private readonly InputAction _command;
        private readonly InputAction _queueModifier;
        private readonly InputAction _cycleFormation;
        private readonly float _scrollUnitsPerNotch;

        private bool _isDisposed;

        public InputSystemPlayerInput()
        {
            _actionMap = new InputActionMap(ActionMapName);
            _pan = CreatePanAction(_actionMap);
            _zoom = _actionMap.AddAction(ZoomActionName, InputActionType.Value, InputBindingPaths.MouseScrollY, expectedControlLayout: AxisControlType);
            _pointer = _actionMap.AddAction(PointerActionName, InputActionType.Value, InputBindingPaths.PointerPosition, expectedControlLayout: Vector2ControlType);
            _select = _actionMap.AddAction(SelectActionName, InputActionType.Button, InputBindingPaths.MouseLeftButton);
            _command = _actionMap.AddAction(CommandActionName, InputActionType.Button, InputBindingPaths.MouseRightButton);
            _queueModifier = CreateQueueModifierAction(_actionMap);
            _cycleFormation = _actionMap.AddAction(CycleFormationActionName, InputActionType.Button, InputBindingPaths.KeyF);
            _scrollUnitsPerNotch = ResolveScrollUnitsPerNotch();
        }

        public NumericsVector2 PanAxis => ToNumerics(_pan.ReadValue<UnityVector2>());

        public float ZoomDelta => _zoom.ReadValue<float>() / _scrollUnitsPerNotch;

        public NumericsVector2 PointerScreenPosition => ToNumerics(_pointer.ReadValue<UnityVector2>());

        public bool WasSelectPressedThisFrame => _select.WasPressedThisFrame();

        public bool IsSelectHeld => _select.IsPressed();

        public bool WasSelectReleasedThisFrame => _select.WasReleasedThisFrame();

        public bool WasCommandPressedThisFrame => _command.WasPressedThisFrame();

        public bool IsQueueModifierHeld => _queueModifier.IsPressed();

        public bool WasCycleFormationPressedThisFrame => _cycleFormation.WasPressedThisFrame();

        public void Enable()
        {
            ThrowIfDisposed();
            _actionMap.Enable();
        }

        public void Disable()
        {
            ThrowIfDisposed();
            _actionMap.Disable();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _actionMap.Disable();
            _actionMap.Dispose();
            _isDisposed = true;
        }

        private static InputAction CreatePanAction(InputActionMap actionMap)
        {
            InputAction pan = actionMap.AddAction(PanActionName, InputActionType.Value, expectedControlLayout: Vector2ControlType);

            // One composite with two bindings per direction, so WASD and the arrow keys
            // combine into a single normalized direction instead of competing for control.
            pan.AddCompositeBinding(InputBindingPaths.Vector2DComposite)
                .With(InputBindingPaths.CompositeUp, InputBindingPaths.KeyW)
                .With(InputBindingPaths.CompositeUp, InputBindingPaths.KeyUpArrow)
                .With(InputBindingPaths.CompositeDown, InputBindingPaths.KeyS)
                .With(InputBindingPaths.CompositeDown, InputBindingPaths.KeyDownArrow)
                .With(InputBindingPaths.CompositeLeft, InputBindingPaths.KeyA)
                .With(InputBindingPaths.CompositeLeft, InputBindingPaths.KeyLeftArrow)
                .With(InputBindingPaths.CompositeRight, InputBindingPaths.KeyD)
                .With(InputBindingPaths.CompositeRight, InputBindingPaths.KeyRightArrow);

            return pan;
        }

        private static InputAction CreateQueueModifierAction(InputActionMap actionMap)
        {
            InputAction queueModifier = actionMap.AddAction(QueueModifierActionName, InputActionType.Button, InputBindingPaths.KeyLeftShift);
            queueModifier.AddBinding(InputBindingPaths.KeyRightShift);
            return queueModifier;
        }

        /// <summary>
        /// Gameplay tunes zoom speed per notch, so the delta must mean the same on every machine.
        /// The Input System's default setting already reports one notch as 1. A project switched to the
        /// platform-specific range gets Windows' raw 120 per notch, which would zoom 120 times too fast;
        /// other platforms report about 1 per notch in either mode.
        /// </summary>
        private static float ResolveScrollUnitsPerNotch()
        {
            if (InputSystem.settings.scrollDeltaBehavior == InputSettings.ScrollDeltaBehavior.UniformAcrossAllPlatforms)
            {
                return UniformScrollUnitsPerNotch;
            }

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                return WindowsRawScrollUnitsPerNotch;
            }

            return UniformScrollUnitsPerNotch;
        }

        private static NumericsVector2 ToNumerics(UnityVector2 value)
        {
            return new NumericsVector2(value.x, value.y);
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(InputSystemPlayerInput));
            }
        }
    }
}
