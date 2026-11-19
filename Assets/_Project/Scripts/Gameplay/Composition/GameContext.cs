using System;
using Stilargo.Core.Input;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Shared services the bootstrapper creates and hands to every installer.
    /// </summary>
    public sealed class GameContext
    {
        public GameContext(IPlayerInput playerInput)
        {
            PlayerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
        }

        public IPlayerInput PlayerInput { get; }
    }
}
