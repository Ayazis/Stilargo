using System.Collections.Generic;
using Stilargo.Core.Selection;
using UnityEngine;

namespace Stilargo.Gameplay.Orders
{
    /// <summary>
    /// Gets the first chance at a right click before it becomes a ground move, e.g. to garrison a building.
    /// </summary>
    public interface IPointerCommandHandler
    {
        /// <summary>True when the click was handled and must not become a move order.</summary>
        bool TryHandle(Ray pointerRay, IReadOnlyList<ISelectable> selected);
    }
}
