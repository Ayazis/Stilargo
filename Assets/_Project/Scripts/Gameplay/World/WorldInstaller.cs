using System;
using Stilargo.Core.City;
using Stilargo.Core.Fog;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.Fog;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Composition root for FL's world features: generates the city and sets up fog of war over it.
    /// The game bootstrapper calls <see cref="Install"/> once, then hands <see cref="VisibilityGrid"/>
    /// to units and enemies (vision sources and hideables).
    /// </summary>
    public sealed class WorldInstaller : MonoBehaviour
    {
        [SerializeField] private int _seed;
        [SerializeField] private CityConfigAsset _cityConfig;
        [SerializeField] private CitySpawner _citySpawner;
        [SerializeField] private FogOverlay _fogOverlay;
        [SerializeField] private FogTicker _fogTicker;

        public CityLayout CityLayout { get; private set; }

        public VisibilityGrid VisibilityGrid { get; private set; }

        public void Install()
        {
            if (CityLayout != null)
            {
                throw new InvalidOperationException($"{nameof(WorldInstaller)} is already installed.");
            }

            RequireReferences();
            CityLayout = new CityGenerator(_seed, _cityConfig.ToConfig()).Generate();
            _citySpawner.Construct(CityLayout);

            // The fog grid shares the city's cells so a revealed cell lines up with a street or plot.
            VisibilityGrid = new VisibilityGrid(CityLayout.Width, CityLayout.Height, CityLayout.CellSize);
            _fogOverlay.Construct(VisibilityGrid);
            _fogTicker.Construct(VisibilityGrid);
        }

        private void RequireReferences()
        {
            if (_cityConfig == null || _citySpawner == null || _fogOverlay == null || _fogTicker == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WorldInstaller)} needs a city config, city spawner, fog overlay and fog ticker assigned.");
            }
        }
    }
}
