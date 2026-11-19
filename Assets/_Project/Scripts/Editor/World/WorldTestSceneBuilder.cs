using System.Linq;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Scavenging;
using Stilargo.Gameplay.World;
using Stilargo.Infrastructure.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace Stilargo.Editor.World
{
    /// <summary>
    /// Generates the temporary WorldTest scene from code, so nobody hand-edits scene YAML. It wires the
    /// World prefab through Ci's GameBootstrapper, exactly as Main will, plus an auto-scavenging scout.
    /// Run from the menu or in batch mode with -executeMethod.
    /// </summary>
    public static class WorldTestSceneBuilder
    {
        private const string Root = "Assets/_Project/WorldTest";
        private const string ScenePath = Root + "/WorldTest.unity";
        private const float CityCentre = 58f;
        private const float CameraHeight = 150f;
        private const float CameraOrthographicSize = 62f;
        private const float LookStraightDown = 90f;
        private const float SunAngle = 50f;
        private const float ScoutHeight = 1f;
        private const float ScoutSpeed = 8f;
        private const float ScoutBaseOffset = 0.5f;

        [MenuItem("Stilargo/Build World Test Scene")]
        public static void Build()
        {
            // NewScene unloads every asset nothing references yet, so open the scene before touching assets.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RecreateFolder();
            WorldInstaller world = InstantiateWorld();
            CreateCamera();
            CreateSun();
            CreateBootstrapper(world);
            CreateTestWiring(world, CreateScout());

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log($"World test scene written to {ScenePath}.");
        }

        // Rebuilding from scratch keeps reruns idempotent; everything in this folder is generated.
        private static void RecreateFolder()
        {
            if (AssetDatabase.IsValidFolder(Root))
            {
                AssetDatabase.DeleteAsset(Root);
            }

            AssetDatabase.CreateFolder("Assets/_Project", "WorldTest");
        }

        private static WorldInstaller InstantiateWorld()
        {
            GameObject prefab = WorldPrefabBuilder.Build();
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            return instance.GetComponent<WorldInstaller>();
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

        private static void CreateBootstrapper(WorldInstaller world)
        {
            GameObject bootstrap = new GameObject("Bootstrap", typeof(GameBootstrapper));
            SerializedFields.AssignArray(bootstrap.GetComponent<GameBootstrapper>(), "_installers", new Object[] { world });
        }

        private static VisionSourceComponent CreateScout()
        {
            GameObject scout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scout.name = "Scout";
            Object.DestroyImmediate(scout.GetComponent<Collider>());
            scout.transform.position = new Vector3(CityCentre, ScoutHeight, CityCentre);

            NavMeshAgent agent = scout.AddComponent<NavMeshAgent>();
            agent.speed = ScoutSpeed;
            agent.baseOffset = ScoutBaseOffset;

            // Enabled by the autopilot once the NavMesh is baked; an agent off the NavMesh logs errors.
            agent.enabled = false;
            scout.AddComponent<ScavengerComponent>();
            scout.AddComponent<WorldTestAutoScavenger>();
            return scout.AddComponent<VisionSourceComponent>();
        }

        private static void CreateTestWiring(WorldInstaller world, VisionSourceComponent scout)
        {
            GameObject wiring = new GameObject("WorldTestBootstrap", typeof(WorldTestBootstrap), typeof(WorldTestHud));
            WorldTestBootstrap bootstrap = wiring.GetComponent<WorldTestBootstrap>();
            SerializedFields.Assign(bootstrap, "_worldInstaller", world);
            SerializedFields.Assign(bootstrap, "_scout", scout);
            SerializedFields.Assign(bootstrap, "_hud", wiring.GetComponent<WorldTestHud>());
        }

        // Appends rather than prepends: Main stays first, because the first scene is the one the exe loads.
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
