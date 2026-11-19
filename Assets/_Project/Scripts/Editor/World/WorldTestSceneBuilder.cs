using System;
using System.Linq;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using Stilargo.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Generates the temporary WorldTest scene and its assets from code, so nobody hand-edits scene YAML.
    /// Run from the menu or in batch mode with -executeMethod.
    /// </summary>
    public static class WorldTestSceneBuilder
    {
        private const string Root = "Assets/_Project/WorldTest";
        private const string ScenePath = Root + "/WorldTest.unity";
        private const string LitShader = "Universal Render Pipeline/Lit";
        private const string UnlitShader = "Universal Render Pipeline/Unlit";
        private const float CityCentre = 58f;
        private const float CameraHeight = 150f;
        private const float CameraOrthographicSize = 62f;
        private const float LookStraightDown = 90f;
        private const float SunAngle = 50f;
        private const float VisionSourceHeight = 1f;
        private const int TransparentRenderQueue = (int)RenderQueue.Transparent;
        private const float ScoutSpeed = 8f;
        private const float ScoutBaseOffset = 0.5f;
        private const float LootCacheSize = 1.2f;
        private const float HomeMarkerSize = 4f;
        private const float HomeMarkerThickness = 0.1f;

        [MenuItem("Stilargo/Build World Test Scene")]
        public static void Build()
        {
            // NewScene unloads every asset nothing references yet, so open the scene before creating assets.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RecreateFolder();
            CreateCamera();
            CreateSun();
            WorldInstaller installer = new GameObject("World", typeof(WorldInstaller), typeof(FogTicker)).GetComponent<WorldInstaller>();
            CreateCity(installer);
            CreateFog(installer);
            CreateScavenging(installer);
            VisionSourceComponent scout = CreateScout();
            CreateBootstrap(installer, scout);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"World test scene written to {ScenePath}.");
        }

        private static void RecreateFolder()
        {
            // Rebuilding from scratch keeps reruns idempotent; everything in this folder is generated.
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
            }

            AssetDatabase.CreateFolder("Assets/_Project", "WorldTest");
        }

        private static Material CreateLitMaterial(string name, Color color)
        {
            Material material = new Material(RequireShader(LitShader)) { color = color };
            AssetDatabase.CreateAsset(material, $"{Root}/{name}.mat");
            return material;
        }

        // URP exposes transparency as material properties plus keywords; the inspector normally sets these.
        private static Material CreateFogMaterial()
        {
            Material material = new Material(RequireShader(UnlitShader));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = TransparentRenderQueue;
            AssetDatabase.CreateAsset(material, $"{Root}/Fog.mat");
            return material;
        }

        private static Shader RequireShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{shaderName}' not found. Is URP the active pipeline?");
            }

            return shader;
        }

        private static GameObject CreateCubePrefab(string name, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cube, $"{Root}/{name}.prefab");
            UnityEngine.Object.DestroyImmediate(cube);
            return prefab;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(CityCentre, CameraHeight, CityCentre),
                Quaternion.Euler(LookStraightDown, 0f, 0f));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = CameraOrthographicSize;
        }

        private static void CreateSun()
        {
            GameObject sun = new GameObject("Sun", typeof(Light));
            sun.GetComponent<Light>().type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(SunAngle, SunAngle, 0f);
        }

        private static void CreateCity(WorldInstaller installer)
        {
            GameObject city = new GameObject("City", typeof(NavMeshSurface), typeof(CitySpawner));

            // Only the spawned city is walkable; markers and units elsewhere must not end up in the bake.
            city.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;
            CitySpawner spawner = city.GetComponent<CitySpawner>();
            Assign(spawner, "_groundPrefab", CreateCubePrefab("Ground", CreateLitMaterial("Ground", new Color(0.25f, 0.25f, 0.27f))));
            Assign(spawner, "_buildingPrefab", CreateCubePrefab("Building", CreateLitMaterial("Building", new Color(0.7f, 0.68f, 0.62f))));

            CityConfigAsset config = ScriptableObject.CreateInstance<CityConfigAsset>();
            AssetDatabase.CreateAsset(config, $"{Root}/CityConfig.asset");
            Assign(installer, "_cityConfig", config);
            Assign(installer, "_citySpawner", spawner);
        }

        private static void CreateFog(WorldInstaller installer)
        {
            GameObject fogQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogQuad.name = "Fog";
            UnityEngine.Object.DestroyImmediate(fogQuad.GetComponent<Collider>());
            fogQuad.GetComponent<Renderer>().sharedMaterial = CreateFogMaterial();
            Assign(installer, "_fogOverlay", fogQuad.AddComponent<FogOverlay>());
            Assign(installer, "_fogTicker", installer.GetComponent<FogTicker>());
        }

        private static void CreateScavenging(WorldInstaller installer)
        {
            LootConfigAsset config = ScriptableObject.CreateInstance<LootConfigAsset>();
            AssetDatabase.CreateAsset(config, $"{Root}/LootConfig.asset");

            GameObject spawnerObject = new GameObject("Loot", typeof(LootCacheSpawner));
            LootCacheSpawner spawner = spawnerObject.GetComponent<LootCacheSpawner>();
            Assign(spawner, "_cachePrefab", CreateLootCachePrefab());

            GameObject home = CreateHomeMarker();

            Assign(installer, "_lootConfig", config);
            Assign(installer, "_lootCacheSpawner", spawner);
            Assign(installer, "_dropOffPoint", home.transform);
        }

        private static LootCacheView CreateLootCachePrefab()
        {
            GameObject cache = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cache.name = "LootCache";
            UnityEngine.Object.DestroyImmediate(cache.GetComponent<Collider>());
            cache.transform.localScale = Vector3.one * LootCacheSize;
            cache.GetComponent<Renderer>().sharedMaterial = CreateLitMaterial("LootCache", new Color(1f, 0.8f, 0.1f));
            cache.AddComponent<LootCacheView>();
            cache.AddComponent<FogHideable>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cache, $"{Root}/LootCache.prefab");
            UnityEngine.Object.DestroyImmediate(cache);
            return prefab.GetComponent<LootCacheView>();
        }

        private static GameObject CreateHomeMarker()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Home (drop-off)";
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.position = new Vector3(CityCentre, HomeMarkerThickness, CityCentre);
            marker.transform.localScale = new Vector3(HomeMarkerSize, HomeMarkerThickness, HomeMarkerSize);
            marker.GetComponent<Renderer>().sharedMaterial = CreateLitMaterial("Home", new Color(0.2f, 0.75f, 0.3f));
            return marker;
        }

        private static VisionSourceComponent CreateScout()
        {
            GameObject scout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scout.name = "Scout";
            UnityEngine.Object.DestroyImmediate(scout.GetComponent<Collider>());
            scout.transform.position = new Vector3(CityCentre, VisionSourceHeight, CityCentre);

            NavMeshAgent agent = scout.AddComponent<NavMeshAgent>();
            agent.speed = ScoutSpeed;
            agent.baseOffset = ScoutBaseOffset;

            // Enabled by the autopilot once the NavMesh is baked; an agent off the NavMesh logs errors.
            agent.enabled = false;
            scout.AddComponent<ScavengerComponent>();
            scout.AddComponent<WorldTestAutoScavenger>();
            return scout.AddComponent<VisionSourceComponent>();
        }

        private static void CreateBootstrap(WorldInstaller installer, VisionSourceComponent scout)
        {
            GameObject bootstrapObject = new GameObject("WorldTestBootstrap", typeof(WorldTestBootstrap), typeof(WorldTestHud));
            WorldTestBootstrap bootstrap = bootstrapObject.GetComponent<WorldTestBootstrap>();
            Assign(bootstrap, "_worldInstaller", installer);
            Assign(bootstrap, "_scout", scout);
            Assign(bootstrap, "_hud", bootstrapObject.GetComponent<WorldTestHud>());
        }

        // Private [SerializeField]s are only reachable through SerializedObject, which is also what the inspector uses.
        private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), $"Nothing to assign to {target.GetType().Name}.{field}.");
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name} has no serialized field '{field}'.");
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (property.objectReferenceValue != value)
            {
                throw new InvalidOperationException($"Could not assign {value} to {target.GetType().Name}.{field}.");
            }
        }

        private static void AddSceneToBuildSettings()
        {
            if (EditorBuildSettings.scenes.Any(scene => scene.path == ScenePath))
            {
                return;
            }

            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Append(new EditorBuildSettingsScene(ScenePath, true))
                .ToArray();
        }
    }
}
