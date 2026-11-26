using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace Stilargo.Gameplay.City
{
    /// <summary>
    /// Places the primitive shapes landmarks are built from under one parent. Small props are left out of the
    /// NavMesh bake, so units walk past barrels and benches instead of getting stuck on them.
    /// </summary>
    internal sealed class PropFactory
    {
        // Unity's cylinder primitive is 2 m tall at scale 1.
        private const float CylinderPrimitiveHeight = 2f;

        private readonly Transform _parent;

        public PropFactory(Transform parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _parent = parent;
        }

        public Transform Parent => _parent;

        public GameObject Box(Vector3 centre, Vector3 size, Material material)
        {
            return Create(PrimitiveType.Cube, centre, size, material);
        }

        public GameObject Cylinder(Vector3 baseCentre, float diameter, float height, Material material)
        {
            Vector3 centre = baseCentre + (Vector3.up * (height * 0.5f));
            return Create(PrimitiveType.Cylinder, centre, new Vector3(diameter, height / CylinderPrimitiveHeight, diameter), material);
        }

        public GameObject Sphere(Vector3 centre, Vector3 size, Material material)
        {
            return Create(PrimitiveType.Sphere, centre, size, material);
        }

        /// <summary>Keeps a prop out of the NavMesh so it never blocks a street or a door.</summary>
        public static GameObject IgnoredByNavigation(GameObject prop)
        {
            prop.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            return prop;
        }

        /// <summary>
        /// Makes a prop pure decoration: out of the NavMesh and without a collider, so it never blocks a unit or
        /// catches a click meant for the street.
        /// </summary>
        public static GameObject Decorative(GameObject prop)
        {
            UnityEngine.Object.Destroy(prop.GetComponent<Collider>());
            return IgnoredByNavigation(prop);
        }

        private GameObject Create(PrimitiveType type, Vector3 centre, Vector3 size, Material material)
        {
            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            GameObject prop = GameObject.CreatePrimitive(type);
            prop.transform.SetParent(_parent, false);
            prop.transform.position = centre;
            prop.transform.localScale = size;
            prop.GetComponent<Renderer>().sharedMaterial = material;
            return prop;
        }
    }
}
