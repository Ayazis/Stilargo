using System;
using Stilargo.Editor.Rendering;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using Stilargo.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Generates <c>World.prefab</c>: the WorldInstaller with its city, fog, loot and drop-off, fully wired.
    /// Drop it into a scene and add its WorldInstaller to the GameBootstrapper's list.
    /// Rerunning keeps existing asset GUIDs and config values, so scenes that use the prefab stay linked.
    /// </summary>
    public static class WorldPrefabBuilder
    {
        public const string PrefabPath = Root + "/World.prefab";

        private const string Root = "Assets/_Project/World";
        private const float CityCentre = 58f;
        private const float LootCacheSize = 1.2f;
        private const float HomeMarkerSize = 4f;
        private const float HomeMarkerThickness = 0.1f;

        // Plain base for the building box; CitySpawner gives each building a plaster colour from the palette.
        private static readonly Color BuildingColor = new Color(0.88f, 0.83f, 0.7f);
        private static readonly Color LootColor = new Color(1f, 0.8f, 0.1f);
        private static readonly Color HomeColor = new Color(0.2f, 0.75f, 0.3f);

        [MenuItem("Stilargo/Build World Prefab")]
        public static GameObject Build()
        {
            EnsureFolder();
            GameObject root = new GameObject("World", typeof(WorldInstaller), typeof(FogTicker));
            try
            {
                WorldInstaller installer = root.GetComponent<WorldInstaller>();
                AddCity(installer);
                AddFog(installer);
                AddScavenging(installer);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"World prefab written to {PrefabPath}.");
                return prefab;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AddCity(WorldInstaller installer)
        {
            GameObject city = CreateChild("City", installer.transform, typeof(NavMeshSurface), typeof(CitySpawner));

            // Only the spawned city is walkable; markers and units elsewhere must not end up in the bake.
            city.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;
            CitySpawner spawner = city.GetComponent<CitySpawner>();
            SerializedFields.Assign(spawner, "_groundPrefab", LoadOrCreateCubePrefab("Ground", CityLookAssets.GroundMaterial()));
            SerializedFields.Assign(spawner, "_buildingPrefab", LoadOrCreateCubePrefab("Building", LoadOrCreateLitMaterial("Building", BuildingColor)));
            CityLookAssets.Dress(spawner);
            SerializedFields.Assign(installer, "_cityConfig", LoadOrCreateAsset<CityConfigAsset>("CityConfig"));
            SerializedFields.Assign(installer, "_citySpawner", spawner);
        }

        private static void AddFog(WorldInstaller installer)
        {
            GameObject fogQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogQuad.name = "Fog";
            fogQuad.transform.SetParent(installer.transform, false);
            UnityEngine.Object.DestroyImmediate(fogQuad.GetComponent<Collider>());
            fogQuad.GetComponent<Renderer>().sharedMaterial = LoadOrCreateFogMaterial();
            SerializedFields.Assign(installer, "_fogOverlay", fogQuad.AddComponent<FogOverlay>());
            SerializedFields.Assign(installer, "_fogTicker", installer.GetComponent<FogTicker>());
        }

        private static void AddScavenging(WorldInstaller installer)
        {
            GameObject loot = CreateChild("Loot", installer.transform, typeof(LootCacheSpawner));
            LootCacheSpawner spawner = loot.GetComponent<LootCacheSpawner>();
            SerializedFields.Assign(spawner, "_cachePrefab", LoadOrCreateLootCachePrefab());

            SerializedFields.Assign(installer, "_lootConfig", LoadOrCreateAsset<LootConfigAsset>("LootConfig"));
            SerializedFields.Assign(installer, "_lootCacheSpawner", spawner);
            SerializedFields.Assign(installer, "_dropOffPoint", CreateHomeMarker(installer.transform).transform);
        }

        // Until step 9 lets the player claim a house, home is a marker at the city centre.
        private static GameObject CreateHomeMarker(Transform parent)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Home (drop-off)";
            marker.transform.SetParent(parent, false);
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.localPosition = new Vector3(CityCentre, HomeMarkerThickness, CityCentre);
            marker.transform.localScale = new Vector3(HomeMarkerSize, HomeMarkerThickness, HomeMarkerSize);
            marker.GetComponent<Renderer>().sharedMaterial = LoadOrCreateLitMaterial("Home", HomeColor);
            return marker;
        }

        private static GameObject CreateChild(string name, Transform parent, params Type[] components)
        {
            GameObject child = new GameObject(name, components);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T LoadOrCreateAsset<T>(string name) where T : ScriptableObject
        {
            string path = $"{Root}/{name}.asset";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material LoadOrCreateLitMaterial(string name, Color color)
        {
            string path = $"{Root}/{name}.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                // Colours are owned by this builder, so a rerun picks up palette changes.
                HdrpMaterials.SetLitColor(existing, color);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Material material = HdrpMaterials.CreateLit(color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material LoadOrCreateFogMaterial()
        {
            string path = $"{Root}/Fog.mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = HdrpMaterials.CreateTransparentUnlit();
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject LoadOrCreateCubePrefab(string name, Material material)
        {
            string path = $"{Root}/{name}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return SaveAndDestroy(cube, path);
        }

        private static LootCacheView LoadOrCreateLootCachePrefab()
        {
            string path = $"{Root}/LootCache.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing.GetComponent<LootCacheView>();
            }

            GameObject cache = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cache.name = "LootCache";
            UnityEngine.Object.DestroyImmediate(cache.GetComponent<Collider>());
            cache.transform.localScale = Vector3.one * LootCacheSize;
            cache.GetComponent<Renderer>().sharedMaterial = LoadOrCreateLitMaterial("LootCache", LootColor);
            cache.AddComponent<LootCacheView>();
            cache.AddComponent<FogHideable>();
            return SaveAndDestroy(cache, path).GetComponent<LootCacheView>();
        }

        private static GameObject SaveAndDestroy(GameObject instance, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }


        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "World");
            }
        }
    }
}
