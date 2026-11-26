using System;
using Stilargo.Core.Input;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Shared services the bootstrapper creates and hands to every installer.
    /// </summary>
    public sealed class GameContext
    {
        public GameContext(IPlayerInput playerInput, GameConfig config, CameraControlChoice cameraControls)
        {
            PlayerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            CameraControls = cameraControls ?? throw new ArgumentNullException(nameof(cameraControls));
        }

        public IPlayerInput PlayerInput { get; }

        public GameConfig Config { get; }

        /// <summary>The camera preset picked in the settings menu; the player input follows it.</summary>
        public CameraControlChoice CameraControls { get; }
    }
}
