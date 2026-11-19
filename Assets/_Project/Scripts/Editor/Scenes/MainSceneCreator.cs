using System;
using System.Linq;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Composition;
using Stilargo.Infrastructure.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stilargo.Editor.Scenes
{
    /// <summary>
    /// Creates the greybox Main scene from code, so it can be made in batch mode
    /// without anyone opening the editor or hand-writing scene YAML.
    /// Run with: -executeMethod Stilargo.Editor.Scenes.MainSceneCreator.CreateIfMissing
    /// </summary>
    public static class MainSceneCreator
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string CameraSettingsPath = SettingsFolder + "/CameraSettings.asset";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string GroundMaterialPath = MaterialsFolder + "/Greybox_Ground.mat";
        private const string LandmarkMaterialPath = MaterialsFolder + "/Greybox_Landmark.mat";
        private const string LitShaderName = "Universal Render Pipeline/Lit";
        private const string BaseColorProperty = "_BaseColor";

        // Distinct colours so the ground and the boxes can be told apart; Unity's default material is plain white.
        private static readonly Color GroundColor = new Color(0.22f, 0.24f, 0.27f);
        private static readonly Color LandmarkColor = new Color(0.85f, 0.45f, 0.15f);

        private const string BootstrapName = "Bootstrap";
        private const string CameraName = "Main Camera";
        private const string MainCameraTag = "MainCamera";
        private const string LightName = "Sun";
        private const string GroundName = "Ground";
        private const string LandmarkName = "Landmark";

        private const string InstallersField = "_installers";
        private const string CameraRigField = "_cameraRig";
        private const string CameraSettingsField = "_cameraSettings";

        // Unity's plane primitive is 10 x 10 units; scale it to cover the default ±100 camera bounds.
        private const float GroundScale = 20f;
        private const float StartHeight = 30f;
        private const float SunPitchDegrees = 50f;
        private const float SunYawDegrees = -30f;

        // A few boxes on a grid so panning and zooming are visible on an otherwise flat ground.
        private const int LandmarkGridHalfCount = 2;
        private const float LandmarkSpacing = 40f;
        private const float LandmarkSize = 6f;

        [MenuItem("Stilargo/Create Main Scene")]
        public static void CreateIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log($"{ScenePath} already exists; leaving it as it is.");
                AddToBuildSettings();
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSun();
            CreateGround(LoadOrCreateMaterial(GroundMaterialPath, GroundColor));
            CreateLandmarks(LoadOrCreateMaterial(LandmarkMaterialPath, LandmarkColor));
            RtsCameraRig rig = CreateCamera();
            CreateBootstrap(rig, LoadOrCreateCameraSettings());

            EnsureFolder(System.IO.Path.GetDirectoryName(ScenePath));
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            AddToBuildSettings();
            Debug.Log($"Created {ScenePath}.");
        }

        private static void CreateSun()
        {
            var sun = new GameObject(LightName);
            sun.AddComponent<Light>().type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(SunPitchDegrees, SunYawDegrees, 0f);
        }

        private static void CreateGround(Material material)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = GroundName;
            ground.transform.localScale = new Vector3(GroundScale, 1f, GroundScale);
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateLandmarks(Material material)
        {
            for (int x = -LandmarkGridHalfCount; x <= LandmarkGridHalfCount; x++)
            {
                for (int z = -LandmarkGridHalfCount; z <= LandmarkGridHalfCount; z++)
                {
                    GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    box.name = $"{LandmarkName} {x},{z}";
                    box.transform.localScale = Vector3.one * LandmarkSize;
                    box.transform.position = new Vector3(x * LandmarkSpacing, LandmarkSize / 2f, z * LandmarkSpacing);
                    box.GetComponent<Renderer>().sharedMaterial = material;
                }
            }
        }

        private static Material LoadOrCreateMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Shader lit = Shader.Find(LitShaderName);
            if (lit == null)
            {
                throw new InvalidOperationException($"Shader '{LitShaderName}' not found; is URP installed?");
            }

            var material = new Material(lit);
            material.SetColor(BaseColorProperty, color);
            EnsureFolder(MaterialsFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static RtsCameraRig CreateCamera()
        {
            var cameraObject = new GameObject(CameraName) { tag = MainCameraTag };
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, StartHeight, 0f);
            return cameraObject.AddComponent<RtsCameraRig>();
        }

        private static void CreateBootstrap(RtsCameraRig rig, CameraSettings settings)
        {
            var bootstrap = new GameObject(BootstrapName);

            // The installer goes on first so it exists when the bootstrapper's list points at it.
            var combatInstaller = bootstrap.AddComponent<CombatInstaller>();
            var installerFields = new SerializedObject(combatInstaller);
            installerFields.FindProperty(CameraRigField).objectReferenceValue = rig;
            installerFields.FindProperty(CameraSettingsField).objectReferenceValue = settings;
            installerFields.ApplyModifiedPropertiesWithoutUndo();

            var bootstrapper = bootstrap.AddComponent<GameBootstrapper>();
            var bootstrapperFields = new SerializedObject(bootstrapper);
            SerializedProperty installers = bootstrapperFields.FindProperty(InstallersField);
            installers.arraySize = 1;
            installers.GetArrayElementAtIndex(0).objectReferenceValue = combatInstaller;
            bootstrapperFields.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CameraSettings LoadOrCreateCameraSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CameraSettings>(CameraSettingsPath);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(SettingsFolder);
            var settings = ScriptableObject.CreateInstance<CameraSettings>();
            AssetDatabase.CreateAsset(settings, CameraSettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        /// <summary>Puts Main first in Build Settings, because the first scene is the one the exe loads at startup.</summary>
        private static void AddToBuildSettings()
        {
            EditorBuildSettingsScene[] others = EditorBuildSettings.scenes
                .Where(buildScene => buildScene.path != ScenePath)
                .ToArray();

            EditorBuildSettings.scenes = others
                .Prepend(new EditorBuildSettingsScene(ScenePath, true))
                .ToArray();
        }

        private static void EnsureFolder(string folder)
        {
            string normalized = folder.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(normalized).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(normalized));
        }
    }
}
