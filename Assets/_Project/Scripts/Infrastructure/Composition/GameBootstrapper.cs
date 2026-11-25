using System;
using Stilargo.Gameplay.Composition;
using Stilargo.Infrastructure.Config;
using Stilargo.Infrastructure.Input;
using UnityEngine;

namespace Stilargo.Infrastructure.Composition
{
    /// <summary>
    /// Composition root: creates the shared services and hands them to every installer.
    /// New areas of the game add an installer instead of editing this class.
    /// </summary>
    [DefaultExecutionOrder(GameBootstrapper.ExecutionOrder)]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        /// <summary>
        /// Runs before default-order gameplay scripts, so everything is wired
        /// before any of them reads input in its own Awake, OnEnable or Update.
        /// </summary>
        public const int ExecutionOrder = -1000;

        [SerializeField] private Installer[] _installers;

        private InputSystemPlayerInput _playerInput;

        private void Awake()
        {
            ValidateInstallers();

            GameConfig config = GameConfigLoader.Load();
            _playerInput = new InputSystemPlayerInput();
            GameContext context = new GameContext(_playerInput, config);

            foreach (Installer installer in _installers)
            {
                installer.Install(context);
            }
        }

        private void OnEnable()
        {
            // Null only when Awake failed; that exception is the one worth reading, so don't add a second.
            if (_playerInput == null)
            {
                return;
            }

            _playerInput.Enable();
        }

        private void OnDisable()
        {
            if (_playerInput == null)
            {
                return;
            }

            _playerInput.Disable();
        }

        private void OnDestroy()
        {
            if (_playerInput == null)
            {
                return;
            }

            _playerInput.Dispose();
            _playerInput = null;
        }

        private void ValidateInstallers()
        {
            if (_installers == null || _installers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"{nameof(GameBootstrapper)} on '{name}' has no installers assigned; nothing would be wired.");
            }

            for (int index = 0; index < _installers.Length; index++)
            {
                if (_installers[index] == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(GameBootstrapper)} on '{name}' has an empty installer slot at index {index}.");
                }
            }
        }
    }
}
