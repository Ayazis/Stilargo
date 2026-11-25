using Stilargo.Core.Hud;
using UnityEngine;

namespace Stilargo.UI.Hud
{
    /// <summary>Quits the player build. Application.Quit does nothing in the editor, which is expected.</summary>
    internal sealed class ApplicationQuitter : IGameQuitter
    {
        public void Quit()
        {
            Application.Quit();
        }
    }
}
