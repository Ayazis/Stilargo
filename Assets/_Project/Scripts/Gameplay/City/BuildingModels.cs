using System;
using System.Collections.Generic;
using Stilargo.Core.City;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The Blender-built building prefabs (<c>Stilargo → Build Canal Town Prefabs</c>), one per house style and one per
    /// landmark. Assigned by <c>Stilargo → Build World Prefab</c>.
    /// </summary>
    [Serializable]
    public sealed class BuildingModels
    {
        [SerializeField] private GameObject _rowHouse;
        [SerializeField] private GameObject _cornerHouse;
        [SerializeField] private GameObject _townhouse;
        [SerializeField] private GameObject _detachedHome;
        [SerializeField] private GameObject _farmhouse;
        [SerializeField] private GameObject _workshop;
        [SerializeField] private GameObject _church;
        [SerializeField] private GameObject _school;
        [SerializeField] private GameObject _tavern;

        public bool IsComplete => AllModels().TrueForAll(model => model != null);

        /// <summary>The model a building plot is drawn with: its landmark if it is one, otherwise its house style.</summary>
        public GameObject For(Plot plot)
        {
            if (plot == null)
            {
                throw new ArgumentNullException(nameof(plot));
            }

            switch (plot.Use)
            {
                case PlotUse.Church:
                    return _church;
                case PlotUse.School:
                    return _school;
                case PlotUse.Tavern:
                    return _tavern;
                case PlotUse.House:
                    return ForStyle(plot.Style);
                default:
                    throw new ArgumentException($"{plot.Use} plots have no building.", nameof(plot));
            }
        }

        private GameObject ForStyle(HouseStyle style)
        {
            switch (style)
            {
                case HouseStyle.Corner:
                    return _cornerHouse;
                case HouseStyle.Townhouse:
                    return _townhouse;
                case HouseStyle.Detached:
                    return _detachedHome;
                case HouseStyle.Farmhouse:
                    return _farmhouse;
                case HouseStyle.Workshop:
                    return _workshop;
                default:
                    return _rowHouse;
            }
        }

        private List<GameObject> AllModels()
        {
            return new List<GameObject> { _rowHouse, _cornerHouse, _townhouse, _detachedHome, _farmhouse, _workshop, _church, _school, _tavern };
        }
    }
}
