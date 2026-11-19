using System.Numerics;

namespace Stilargo.Core.Orders
{
    /// <summary>
    /// The movement backend of one unit (a NavMeshAgent in Gameplay). Positions are world space, Y up.
    /// </summary>
    public interface INavigationAgent
    {
        void SetDestination(Vector3 destination);

        void Halt();
    }
}
