using System;
using Stilargo.Core.Input;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Shared services the bootstrapper creates and hands to every installer.
    /// </summary>
    public sealed class GameContext
    {
        public GameContext(IPlayerInput playerInput, GameConfig config)
        {
            PlayerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IPlayerInput PlayerInput { get; }

        public GameConfig Config { get; }
    }
}
