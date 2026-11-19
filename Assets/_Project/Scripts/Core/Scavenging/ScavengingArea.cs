using System;
using System.Collections.Generic;
using Stilargo.Core.Fog;

namespace Stilargo.Core.Scavenging
{
    /// <summary>
    /// The scavenging rules: an empty carrier within reach of a cache picks it up, and a loaded carrier within
    /// reach of the drop-off delivers. Proximity-based, so it works with whatever moves the unit.
    /// </summary>
    public sealed class ScavengingArea
    {
        private readonly float _reachSquared;

        public ScavengingArea(IReadOnlyList<LootCache> caches, DropOff dropOff, float reachRadius)
        {
            if (!(reachRadius > 0f) || float.IsInfinity(reachRadius))
            {
                throw new ArgumentOutOfRangeException(nameof(reachRadius), reachRadius, "Reach must be a positive finite number.");
            }

            Caches = caches ?? throw new ArgumentNullException(nameof(caches));
            DropOff = dropOff ?? throw new ArgumentNullException(nameof(dropOff));
            _reachSquared = reachRadius * reachRadius;
        }

        public IReadOnlyList<LootCache> Caches { get; }

        public DropOff DropOff { get; }

        public ScavengeResult Visit(LootCarrier carrier, WorldPoint position)
        {
            if (carrier == null)
            {
                throw new ArgumentNullException(nameof(carrier));
            }

            if (carrier.IsCarrying)
            {
                return TryDeliver(carrier, position);
            }

            return TryPickUp(carrier, position);
        }

        /// <summary>The closest cache not yet looted, or null when everything is taken.</summary>
        public LootCache FindNearestAvailable(WorldPoint position)
        {
            LootCache nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (LootCache cache in Caches)
            {
                float distance = DistanceSquared(cache.Position, position);
                if (!cache.IsLooted && distance < nearestDistance)
                {
                    nearest = cache;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private ScavengeResult TryDeliver(LootCarrier carrier, WorldPoint position)
        {
            if (!IsWithinReach(DropOff.Position, position))
            {
                return ScavengeResult.Nothing;
            }

            carrier.DeliverTo(DropOff.Wallet);
            return ScavengeResult.Delivered;
        }

        private ScavengeResult TryPickUp(LootCarrier carrier, WorldPoint position)
        {
            LootCache nearest = FindNearestAvailable(position);
            if (nearest == null || !IsWithinReach(nearest.Position, position))
            {
                return ScavengeResult.Nothing;
            }

            carrier.PickUp(nearest);
            return ScavengeResult.PickedUp;
        }

        private bool IsWithinReach(WorldPoint target, WorldPoint position)
        {
            return DistanceSquared(target, position) <= _reachSquared;
        }

        private static float DistanceSquared(WorldPoint a, WorldPoint b)
        {
            float dx = a.X - b.X;
            float dz = a.Z - b.Z;
            return (dx * dx) + (dz * dz);
        }
    }
}
