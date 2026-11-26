using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using Stilargo.Core.Economy;
using Stilargo.Core.Fog;
using Stilargo.Core.Scavenging;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using UnityEngine;

namespace Stilargo.Gameplay.World
{
    /// <summary>
    /// Wires FL's side of the game: generates the city, sets up fog of war over it and scatters loot for
    /// scavenging. List it before installers that need <see cref="VisibilityGrid"/> or
    /// <see cref="ScavengingArea"/>, because those exist only after <see cref="Install"/> has run.
    /// </summary>
    public sealed class WorldInstaller : Installer
    {
        [SerializeField] private int _seed;
        [SerializeField] private CityConfigAsset _cityConfig;
        [SerializeField] private CitySpawner _citySpawner;
        [SerializeField] private FogOverlay _fogOverlay;
        [SerializeField] private FogTicker _fogTicker;
        [SerializeField] private LootConfigAsset _lootConfig;
        [SerializeField] private LootCacheSpawner _lootCacheSpawner;
        [SerializeField] private Transform _dropOffPoint;
        [SerializeField] private float _scavengeReach = 2.5f;

        public CityLayout CityLayout { get; private set; }

        public VisibilityGrid VisibilityGrid { get; private set; }

        public Wallet Wallet { get; private set; }

        public ScavengingArea ScavengingArea { get; private set; }

        public override void Install(GameContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (CityLayout != null)
            {
                throw new InvalidOperationException($"{nameof(WorldInstaller)} is already installed.");
            }

            RequireReferences();
            InstallCity(context.Config.Town, context.Config.District);
            InstallFog();
            InstallScavenging(context.Config.Economy);
        }

        private void InstallCity(TownSettings town, DistrictSettings district)
        {
            CityLayout generated = new CityGenerator(_seed, _cityConfig.ToConfig(), town).Generate();
            CityLayout = new PlotUsePlanner(district).Plan(generated, _seed);
            _citySpawner.Construct(CityLayout);
        }

        // The fog grid shares the city's cells so a revealed cell lines up with a street or plot. Buildings block its view.
        private void InstallFog()
        {
            VisibilityGrid = new VisibilityGrid(CityLayout.Width, CityLayout.Height, CityLayout.CellSize);
            VisibilityGrid.UseSightBlocker(new CitySkyline(CityLayout, _citySpawner.StoreyHeightMetres));
            _fogOverlay.Construct(VisibilityGrid);
            _fogTicker.Construct(VisibilityGrid);
        }

        private void InstallScavenging(EconomySettings economy)
        {
            Wallet = new Wallet();
            if (economy.StartingMoney > 0)
            {
                Wallet.Deposit(economy.StartingMoney);
            }

            Vector3 home = _dropOffPoint.position;
            DropOff dropOff = new DropOff(new WorldPoint(home.x, home.z), Wallet);
            IReadOnlyList<LootCache> caches = new LootPlacer(_seed, _lootConfig.ToConfig()).Place(CityLayout);
            ScavengingArea = new ScavengingArea(caches, dropOff, _scavengeReach);
            _lootCacheSpawner.Construct(caches, VisibilityGrid);
        }

        private void RequireReferences()
        {
            if (_cityConfig == null || _citySpawner == null || _fogOverlay == null || _fogTicker == null
                || _lootConfig == null || _lootCacheSpawner == null || _dropOffPoint == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WorldInstaller)} needs every city, fog and scavenging reference assigned.");
            }
        }
    }
}
