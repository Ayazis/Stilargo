using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using Stilargo.Core.City.CanalTown;
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
    /// Wires FL's side of the game: generates the canal town (and where each side's soldiers enter), sets up fog of war over it and scatters loot for
    /// scavenging. List it before installers that need <see cref="VisibilityGrid"/> or
    /// <see cref="ScavengingArea"/>, because those exist only after <see cref="Install"/> has run.
    /// </summary>
    public sealed class WorldInstaller : Installer
    {
        private const float CellCentre = 0.5f;

        [SerializeField] private int _seed;
        [SerializeField] private CitySpawner _citySpawner;
        [SerializeField] private FogOverlay _fogOverlay;
        [SerializeField] private FogTicker _fogTicker;
        [SerializeField] private LootConfigAsset _lootConfig;
        [SerializeField] private LootCacheSpawner _lootCacheSpawner;
        [SerializeField] private Transform _dropOffPoint;
        [SerializeField] private float _scavengeReach = 2.5f;

        public CityLayout CityLayout { get; private set; }

        /// <summary>Ground point on the west map edge where the player's soldiers enter; also their drop-off.</summary>
        public Vector3 PlayerEntry { get; private set; }

        /// <summary>Ground point on the east map edge where the rival's soldiers enter.</summary>
        public Vector3 RivalEntry { get; private set; }

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
            InstallFog(context.Config.Vision);
            InstallScavenging(context.Config.Economy);
        }

        private void InstallCity(TownSettings town, DistrictSettings district)
        {
            CanalTown canalTown = new CanalTownGenerator(_seed, town.CanalTown).Generate();
            CityLayout = new PlotUsePlanner(district).Plan(canalTown.Layout, _seed);
            PlayerEntry = GroundPointOf(canalTown.PlayerEntry);
            RivalEntry = GroundPointOf(canalTown.RivalEntry);
            _dropOffPoint.position = new Vector3(PlayerEntry.x, _dropOffPoint.position.y, PlayerEntry.z);
            List<StreetProp> streetProps = new StreetFurniturePlanner(town.StreetLife).Plan(CityLayout, _seed);
            _citySpawner.Construct(CityLayout, streetProps, canalTown);
        }

        private Vector3 GroundPointOf(GridPosition cell)
        {
            return new Vector3((cell.X + CellCentre) * CityLayout.CellSize, 0f, (cell.Y + CellCentre) * CityLayout.CellSize);
        }

        // The fog grid splits each city cell into finer fog cells, so revealed edges look round instead of blocky,
        // while each fog cell still lines up inside one street or plot. Buildings block its view.
        private void InstallFog(VisionSettings vision)
        {
            int split = vision.FogCellsPerCityCell;
            VisibilityGrid = new VisibilityGrid(CityLayout.Width * split, CityLayout.Height * split, CityLayout.CellSize / split);
            var skyline = new CitySkyline(CityLayout, _citySpawner.StoreyHeightMetres);
            VisibilityGrid.UseSightBlocker(new SubdividedSightBlocker(skyline, split));
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
            if (_citySpawner == null || _fogOverlay == null || _fogTicker == null
                || _lootConfig == null || _lootCacheSpawner == null || _dropOffPoint == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(WorldInstaller)} needs every city, fog and scavenging reference assigned.");
            }
        }
    }
}
