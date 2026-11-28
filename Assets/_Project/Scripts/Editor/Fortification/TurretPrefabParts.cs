using System;
using Stilargo.Editor.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Editor.Fortification
{
    /// <summary>
    /// The code-built parts added to the turret model: the level deck it stands on, and the muzzle flash (a glowing
    /// burst plus a short-lived point light). Their materials are placeholders that can be swapped for authored ones.
    /// </summary>
    internal static class TurretPrefabParts
    {
        public const float DeckHalfSizeMetres = 0.7f;

        private const string DeckMaterialPath = "Assets/_Project/Art/Models/Turret/TurretDeck.mat";
        private const string FlashMaterialPath = "Assets/_Project/Art/Models/Turret/MuzzleFlash.mat";

        // Deep enough to reach a 26.6-degree roof from the deck's top edge to its far corner.
        private const float DeckDepthMetres = 0.9f;
        private const float DeckSmoothness = 0.3f;

        // A burst a little wider than the twin barrels and longer than it is wide.
        private static readonly Vector3 FlashSizeMetres = new Vector3(0.35f, 0.35f, 0.6f);

        // Unlit surfaces are not scaled by auto exposure, so the flash colour must be bright enough to read in
        // full sun (about EV 14). This multiplier puts its core well above the bloom threshold.
        private const float FlashLuminanceMultiplier = 60000f;
        private const float FlashLightLumen = 60000f;
        private const float FlashLightRangeMetres = 8f;
        private const float FlashTemperatureKelvin = 2200f;

        // Dark steel plate over timber; inside the PBR albedo range.
        private static readonly Color DeckColor = new Color(0.16f, 0.15f, 0.13f);
        private static readonly Color FlashColor = new Color(1f, 0.72f, 0.35f);

        public static void AddDeck(Transform root)
        {
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.DestroyImmediate(deck.GetComponent<Collider>());
            deck.name = "Deck";
            deck.transform.SetParent(root, false);
            deck.transform.localScale = new Vector3(DeckHalfSizeMetres * 2f, DeckDepthMetres, DeckHalfSizeMetres * 2f);
            deck.transform.localPosition = Vector3.down * (DeckDepthMetres / 2f);
            deck.GetComponent<Renderer>().sharedMaterial = LoadOrCreate(DeckMaterialPath, CreateDeckMaterial);
        }

        /// <summary>Adds the flash under the muzzle, pointing along the barrels, and returns it.</summary>
        public static GameObject AddFlash(Transform muzzle, Vector3 barrelDirection)
        {
            var flash = new GameObject("MuzzleFlash");
            flash.transform.SetParent(muzzle, false);
            flash.transform.rotation = Quaternion.LookRotation(barrelDirection);

            GameObject burst = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.DestroyImmediate(burst.GetComponent<Collider>());
            burst.name = "Burst";
            burst.transform.SetParent(flash.transform, false);
            burst.transform.localScale = FlashSizeMetres;
            burst.transform.localPosition = Vector3.forward * (FlashSizeMetres.z / 2f);
            var renderer = burst.GetComponent<Renderer>();
            renderer.sharedMaterial = LoadOrCreate(FlashMaterialPath, () => HdrpMaterials.CreateUnlit(FlashColor * FlashLuminanceMultiplier));
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            AddFlashLight(flash);
            return flash;
        }

        private static void AddFlashLight(GameObject flash)
        {
            flash.AddHDLight(LightType.Point);
            Light light = flash.GetComponent<Light>();
            light.useColorTemperature = true;
            light.colorTemperature = FlashTemperatureKelvin;
            light.color = Color.white;
            light.lightUnit = LightUnit.Lumen;
            light.intensity = FlashLightLumen;
            light.range = FlashLightRangeMetres;
            light.shadows = LightShadows.None;
        }

        private static Material CreateDeckMaterial()
        {
            Material material = HdrpMaterials.CreateLit(DeckColor);
            HdrpMaterials.SetSmoothness(material, DeckSmoothness);
            return material;
        }

        private static Material LoadOrCreate(string path, Func<Material> create)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = create();
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
