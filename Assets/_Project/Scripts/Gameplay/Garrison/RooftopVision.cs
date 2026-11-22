using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Garrison
{
    /// <summary>Extra fog reveal for a unit standing on a roof; follows the unit's transform.</summary>
    public sealed class RooftopVision : IVisionSource
    {
        private readonly Transform _transform;

        public RooftopVision(Transform transform, float visionRadius)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform));
            }

            _transform = transform;
            VisionRadius = visionRadius;
        }

        public WorldPoint Position => new WorldPoint(_transform.position.x, _transform.position.z);

        public float VisionRadius { get; }
    }
}
