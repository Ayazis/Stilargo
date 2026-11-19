using System;
using Stilargo.Core.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.Fog
{
    /// <summary>Refreshes the visibility grid at a fixed interval rather than every frame.</summary>
    public sealed class FogTicker : MonoBehaviour
    {
        [SerializeField] private float _refreshInterval = 0.2f;

        private VisibilityGrid _grid;
        private float _secondsUntilRefresh;

        public void Construct(VisibilityGrid grid)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            if (_grid != null)
            {
                throw new InvalidOperationException($"{nameof(FogTicker)} is already constructed.");
            }

            _grid = grid;
            _secondsUntilRefresh = 0f;
        }

        private void Update()
        {
            if (_grid == null)
            {
                return;
            }

            _secondsUntilRefresh -= Time.deltaTime;
            if (_secondsUntilRefresh > 0f)
            {
                return;
            }

            _secondsUntilRefresh = _refreshInterval;
            _grid.Refresh();
        }
    }
}
