using UnityEngine;

namespace Stilargo.Gameplay.Composition
{
    /// <summary>
    /// Wires one area of the game. Each person owns their own installer
    /// (CombatInstaller, WorldInstaller) so the bootstrapper never has to change.
    /// </summary>
    public abstract class Installer : MonoBehaviour
    {
        public abstract void Install(GameContext context);
    }
}
