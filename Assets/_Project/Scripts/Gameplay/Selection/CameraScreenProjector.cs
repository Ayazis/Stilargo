using System;
using Stilargo.Core.Selection;

namespace Stilargo.Gameplay.Selection
{
    /// <summary>
    /// Projects world positions to screen pixels through a Unity camera.
    /// </summary>
    public sealed class CameraScreenProjector : IScreenProjector
    {
        private readonly UnityEngine.Camera _camera;

        public CameraScreenProjector(UnityEngine.Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _camera = camera;
        }

        public bool TryProject(System.Numerics.Vector3 world, out System.Numerics.Vector2 screen)
        {
            UnityEngine.Vector3 projected = _camera.WorldToScreenPoint(world.ToUnity());
            screen = projected.ToScreenNumerics();
            return projected.z > 0f;
        }
    }
}
