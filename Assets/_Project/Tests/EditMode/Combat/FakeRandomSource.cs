using System.Collections.Generic;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    /// <summary>Returns queued values in order, then repeats the fallback value.</summary>
    public sealed class FakeRandomSource : IRandomSource
    {
        private readonly Queue<float> _queued;
        private readonly float _fallback;

        public FakeRandomSource(float fallback, params float[] queued)
        {
            _fallback = fallback;
            _queued = new Queue<float>(queued);
        }

        public float NextFloat()
        {
            if (_queued.Count == 0)
            {
                return _fallback;
            }

            return _queued.Dequeue();
        }
    }
}
