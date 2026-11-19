using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Selection;

namespace Stilargo.Tests.EditMode.Selection
{
    /// <summary>
    /// A projector whose results each test sets up per world point. Unknown points count as behind the camera.
    /// </summary>
    public sealed class FakeScreenProjector : IScreenProjector
    {
        private readonly Dictionary<Vector3, Vector2> _screenByWorld = new Dictionary<Vector3, Vector2>();

        public void Place(Vector3 world, Vector2 screen)
        {
            _screenByWorld[world] = screen;
        }

        public bool TryProject(Vector3 world, out Vector2 screen)
        {
            return _screenByWorld.TryGetValue(world, out screen);
        }
    }
}
