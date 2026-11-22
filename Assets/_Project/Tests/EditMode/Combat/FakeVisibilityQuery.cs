using System.Collections.Generic;
using System.Numerics;
using Stilargo.Core.Combat;

namespace Stilargo.Tests.EditMode.Combat
{
    /// <summary>Everything is visible except the positions explicitly hidden.</summary>
    public sealed class FakeVisibilityQuery : IVisibilityQuery
    {
        private readonly HashSet<Vector3> _hidden = new HashSet<Vector3>();

        public void Hide(Vector3 position)
        {
            _hidden.Add(position);
        }

        public bool IsVisible(Vector3 position)
        {
            return !_hidden.Contains(position);
        }
    }
}
