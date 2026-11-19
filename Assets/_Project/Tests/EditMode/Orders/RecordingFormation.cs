using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Formations;

namespace Stilargo.Tests.EditMode.Orders
{
    /// <summary>
    /// Remembers the last request and delegates the actual layout to a real formation.
    /// </summary>
    public sealed class RecordingFormation : IFormation
    {
        private readonly IFormation _inner;

        public RecordingFormation(IFormation inner)
        {
            _inner = inner;
        }

        public int LastCount { get; private set; }

        public Vector3 LastCenter { get; private set; }

        public Vector3 LastFacing { get; private set; }

        public IReadOnlyList<Vector3> SlotsFor(int count, Vector3 center, Vector3 facing)
        {
            LastCount = count;
            LastCenter = center;
            LastFacing = facing;
            return _inner.SlotsFor(count, center, facing);
        }
    }
}
