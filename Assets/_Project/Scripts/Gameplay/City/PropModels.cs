using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// The prefab for each <see cref="PropKind"/>, assigned by <c>Stilargo → Build World Prefab</c>. Props are pure
    /// decoration: they are placed without colliders and kept out of the NavMesh.
    /// </summary>
    [Serializable]
    public sealed class PropModels
    {
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();

        private Dictionary<PropKind, GameObject> _byKind;

        public bool IsComplete => Enum.GetValues(typeof(PropKind)).Cast<PropKind>().All(kind => Lookup().TryGetValue(kind, out GameObject prefab) && prefab != null);

        public GameObject this[PropKind kind]
        {
            get
            {
                if (!Lookup().TryGetValue(kind, out GameObject prefab) || prefab == null)
                {
                    throw new InvalidOperationException($"No prop model for {kind}. Run Stilargo → Build World Prefab.");
                }

                return prefab;
            }
        }

        private Dictionary<PropKind, GameObject> Lookup()
        {
            if (_byKind == null)
            {
                _byKind = _entries.ToDictionary(entry => entry.Kind, entry => entry.Prefab);
            }

            return _byKind;
        }

        [Serializable]
        private struct Entry
        {
            [SerializeField] private PropKind _kind;
            [SerializeField] private GameObject _prefab;

            public PropKind Kind => _kind;

            public GameObject Prefab => _prefab;
        }
    }
}
