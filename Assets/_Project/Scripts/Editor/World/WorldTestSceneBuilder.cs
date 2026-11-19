using System;
using System.Linq;
using Stilargo.Gameplay.City;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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

        [MenuItem("Stilargo/Build World Test Scene")]
        public static void Build()
        {
            // NewScene unloads every asset nothing references yet, so open the scene before creating assets.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RecreateFolder();
            GameObject groundPrefab = CreateCubePrefab("Ground", CreateLitMaterial("Ground", new Color(0.25f, 0.25f, 0.27f)));
            GameObject buildingPrefab = CreateCubePrefab("Building", CreateLitMaterial("Building", new Color(0.7f, 0.68f, 0.62f)));
            CityConfigAsset config = CreateCityConfig();
            Material fogMaterial = CreateFogMaterial();

            CreateCamera();
            CreateSun();
            WorldInstaller installer = CreateWorld(config, groundPrefab, buildingPrefab, fogMaterial);
            VisionSourceComponent visionSource = CreateVisionSource();
            CreateBootstrap(installer, visionSource);

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

        private static CityConfigAsset CreateCityConfig()
        {
            CityConfigAsset config = ScriptableObject.CreateInstance<CityConfigAsset>();
            AssetDatabase.CreateAsset(config, $"{Root}/CityConfig.asset");
            return config;
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

        private static WorldInstaller CreateWorld(CityConfigAsset config, GameObject ground, GameObject building, Material fog)
        {
            GameObject city = new GameObject("City", typeof(NavMeshSurface), typeof(CitySpawner));
            CitySpawner spawner = city.GetComponent<CitySpawner>();
            Assign(spawner, "_groundPrefab", ground);
            Assign(spawner, "_buildingPrefab", building);

            GameObject fogQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogQuad.name = "Fog";
            UnityEngine.Object.DestroyImmediate(fogQuad.GetComponent<Collider>());
            fogQuad.GetComponent<Renderer>().sharedMaterial = fog;
            FogOverlay overlay = fogQuad.AddComponent<FogOverlay>();

            GameObject world = new GameObject("World", typeof(WorldInstaller), typeof(FogTicker));
            WorldInstaller installer = world.GetComponent<WorldInstaller>();
            Assign(installer, "_cityConfig", config);
            Assign(installer, "_citySpawner", spawner);
            Assign(installer, "_fogOverlay", overlay);
            Assign(installer, "_fogTicker", world.GetComponent<FogTicker>());
            return installer;
        }

        private static VisionSourceComponent CreateVisionSource()
        {
            GameObject scout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scout.name = "Scout (vision source)";
            UnityEngine.Object.DestroyImmediate(scout.GetComponent<Collider>());
            scout.transform.position = new Vector3(CityCentre, VisionSourceHeight, CityCentre);
            return scout.AddComponent<VisionSourceComponent>();
        }

        private static void CreateBootstrap(WorldInstaller installer, VisionSourceComponent visionSource)
        {
            GameObject bootstrapObject = new GameObject("WorldTestBootstrap", typeof(WorldTestBootstrap));
            SerializedObject bootstrap = new SerializedObject(bootstrapObject.GetComponent<WorldTestBootstrap>());
            bootstrap.FindProperty("_worldInstaller").objectReferenceValue = installer;
            SerializedProperty sources = bootstrap.FindProperty("_visionSources");
            sources.arraySize = 1;
            sources.GetArrayElementAtIndex(0).objectReferenceValue = visionSource;
            bootstrap.ApplyModifiedPropertiesWithoutUndo();
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
