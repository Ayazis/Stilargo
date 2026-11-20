using System;
using System.Linq;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Navigation;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Selection;
using Stilargo.Gameplay.Stances;
using Stilargo.Gameplay.Units;
using Stilargo.Editor.Rendering;
using Stilargo.Infrastructure.Composition;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Stilargo.Editor.Scenes
{
    /// <summary>
    /// Creates the greybox Main scene from code, so it can be made in batch mode
    /// without anyone opening the editor or hand-writing scene YAML.
    /// Run with: -executeMethod Stilargo.Editor.Scenes.MainSceneCreator.CreateIfMissing
    /// or, to rebuild it after this file changes: -executeMethod Stilargo.Editor.Scenes.MainSceneCreator.Recreate
    /// </summary>
    public static class MainSceneCreator
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string CameraSettingsPath = SettingsFolder + "/CameraSettings.asset";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string GroundMaterialPath = MaterialsFolder + "/Greybox_Ground.mat";
        private const string LandmarkMaterialPath = MaterialsFolder + "/Greybox_Landmark.mat";
        private const string UnitMaterialPath = MaterialsFolder + "/Greybox_Unit.mat";
        private const string SelectionMarkerMaterialPath = MaterialsFolder + "/Greybox_SelectionMarker.mat";
        private const string WaypointPathMaterialPath = MaterialsFolder + "/Greybox_WaypointPath.mat";

        // Distinct colours so the ground, boxes, units and their markers can be told apart; Unity's default material is plain white.
        private static readonly Color GroundColor = new Color(0.22f, 0.24f, 0.27f);
        private static readonly Color LandmarkColor = new Color(0.85f, 0.45f, 0.15f);
        private static readonly Color UnitColor = new Color(0.25f, 0.55f, 0.95f);
        private static readonly Color SelectionMarkerColor = new Color(0.3f, 1f, 0.4f);
        private static readonly Color WaypointPathColor = new Color(1f, 0.9f, 0.2f);

        // Enemies read as hostile red against the blue squad; within PBR albedo range (no channel near 0 or 1).
        private static readonly Color EnemyColor = new Color(0.72f, 0.16f, 0.13f);
        private static readonly Color EnemyMarkerColor = new Color(0.94f, 0.22f, 0.16f);
        private static readonly Color PlayerTracerColor = new Color(0.94f, 0.86f, 0.55f);
        private static readonly Color EnemyTracerColor = new Color(0.94f, 0.42f, 0.2f);

        private const string WeaponsFolder = SettingsFolder + "/Weapons";
        private const string PlayerWeaponPath = WeaponsFolder + "/PlayerRifle.asset";
        private const string EnemyWeaponPath = WeaponsFolder + "/EnemyRifle.asset";
        private const string EnemyMaterialPath = MaterialsFolder + "/Greybox_Enemy.mat";
        private const string EnemyMarkerMaterialPath = MaterialsFolder + "/Greybox_EnemyMarker.mat";
        private const string PlayerTracerMaterialPath = MaterialsFolder + "/Greybox_PlayerTracer.mat";
        private const string EnemyTracerMaterialPath = MaterialsFolder + "/Greybox_EnemyTracer.mat";

        private const string EnemyName = "Enemy";
        private const string EnemyMarkerName = "Enemy Marker";
        private const string MuzzleName = "Muzzle";

        private const string EnemiesField = "_enemies";
        private const string PlayerSideField = "_playerSide";
        private const string EnemySideField = "_enemySide";
        private const string SideWeaponField = "_weapon";
        private const string SideMaxHealthField = "_maxHealth";
        private const string StanceControllerField = "_stanceController";
        private const string StanceLabelViewField = "_stanceLabelView";
        private const string LineRendererField = "_lineRenderer";
        private const string WeaponDamageField = "_damage";

        // Enemies stand in a row beyond the squad, between the landmark boxes, out of sight at the start.
        private static readonly float[] EnemyColumnsX = { -14f, -8f, 8f, 14f };
        private const float EnemyRowZ = 40f;
        private const float EnemyMaxHealth = 60f;
        private const float PlayerWeaponDamage = 10f;
        private const float EnemyWeaponDamage = 6f;
        private const float EnemyMarkerDiameter = 1.6f;
        private const float MuzzleHeightAboveCentre = 0.4f;
        private const float TracerWidth = 0.06f;

        private const string BootstrapName = "Bootstrap";
        private const string ControlsName = "Player Controls";
        private const string CameraName = "Main Camera";
        private const string MainCameraTag = "MainCamera";
        private const string LightName = "Sun";
        private const string WalkableWorldName = "Walkable World";
        private const string GroundName = "Ground";
        private const string LandmarkName = "Landmark";
        private const string UnitName = "Unit";
        private const string SelectionMarkerName = "Selection Marker";
        private const string WaypointPathName = "Waypoint Path";

        private const string InstallersField = "_installers";
        private const string CameraRigField = "_cameraRig";
        private const string CameraSettingsField = "_cameraSettings";
        private const string CameraField = "_camera";
        private const string UnitsField = "_units";
        private const string SelectionControllerField = "_selectionController";
        private const string SelectionBoxViewField = "_selectionBoxView";
        private const string MoveOrderControllerField = "_moveOrderController";
        private const string FormationLabelViewField = "_formationLabelView";
        private const string SelectionMarkerField = "_selectionMarker";
        private const string PathViewField = "_pathView";

        // Unity's plane primitive is 10 x 10 units; scale it to cover the default ±100 camera bounds.
        private const float GroundScale = 20f;
        private const float StartHeight = 30f;
        private const float SunPitchDegrees = 50f;
        private const float SunYawDegrees = -30f;

        // A few boxes on a grid so panning and zooming are visible, and so units have obstacles to path around.
        private const int LandmarkGridHalfCount = 2;
        private const float LandmarkSpacing = 40f;
        private const float LandmarkSize = 6f;

        // A small squad placed in a loose grid in front of the camera's start position.
        private const int UnitColumns = 3;
        private const int UnitRows = 2;
        private const float UnitSpacing = 3f;
        private const float UnitStartForward = 12f;
        private const float UnitHalfHeight = 1f;
        private const float UnitAgentSpeed = 8f;
        private const float UnitAgentAcceleration = 40f;

        private const float SelectionMarkerDiameter = 1.8f;
        private const float SelectionMarkerThickness = 0.02f;
        private const float WaypointPathWidth = 0.1f;

        [MenuItem("Stilargo/Create Main Scene")]
        public static void CreateIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                Debug.Log($"{ScenePath} already exists; leaving it as it is.");
                AddToBuildSettings();
                return;
            }

            Create();
        }

        [MenuItem("Stilargo/Recreate Main Scene")]
        public static void Recreate()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null && !AssetDatabase.DeleteAsset(ScenePath))
            {
                throw new InvalidOperationException($"Could not delete {ScenePath} to recreate it.");
            }

            Create();
        }

        private static void Create()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSun();
            PhotorealSceneDefaults.CreateGlobalVolume();
            GroundNavMeshInstaller groundInstaller = CreateWalkableWorld();
            SelectableUnit[] units = CreateUnits();
            CombatUnit[] enemies = CreateEnemies();
            RtsCameraRig rig = CreateCamera();
            CombatInstaller combatInstaller = CreateCombatInstaller(rig, units);
            AssignCombat(combatInstaller, enemies);
            CreateBootstrapper(groundInstaller, combatInstaller);

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
            Light sun = PhotorealSceneDefaults.CreateSun(LightName);
            sun.transform.rotation = Quaternion.Euler(SunPitchDegrees, SunYawDegrees, 0f);
        }

        /// <summary>
        /// Temporary walkable ground for Main until FL's city NavMesh is wired in.
        /// It bakes only its own children, so units are not baked in as obstacles, and from
        /// physics colliders, so the build doesn't depend on mesh read access.
        /// </summary>
        private static GroundNavMeshInstaller CreateWalkableWorld()
        {
            var world = new GameObject(WalkableWorldName);
            var surface = world.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            CreateGround(world.transform, LoadOrCreateLitMaterial(GroundMaterialPath, GroundColor));
            CreateLandmarks(world.transform, LoadOrCreateLitMaterial(LandmarkMaterialPath, LandmarkColor));
            return world.AddComponent<GroundNavMeshInstaller>();
        }

        private static void CreateGround(Transform parent, Material material)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = GroundName;
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(GroundScale, 1f, GroundScale);
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateLandmarks(Transform parent, Material material)
        {
            for (int x = -LandmarkGridHalfCount; x <= LandmarkGridHalfCount; x++)
            {
                for (int z = -LandmarkGridHalfCount; z <= LandmarkGridHalfCount; z++)
                {
                    GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    box.name = $"{LandmarkName} {x},{z}";
                    box.transform.SetParent(parent, false);
                    box.transform.localScale = Vector3.one * LandmarkSize;
                    box.transform.position = new Vector3(x * LandmarkSpacing, LandmarkSize / 2f, z * LandmarkSpacing);
                    box.GetComponent<Renderer>().sharedMaterial = material;
                }
            }
        }

        private static SelectableUnit[] CreateUnits()
        {
            Material body = LoadOrCreateLitMaterial(UnitMaterialPath, UnitColor);
            Material marker = LoadOrCreateUnlitMaterial(SelectionMarkerMaterialPath, SelectionMarkerColor);
            Material path = LoadOrCreateUnlitMaterial(WaypointPathMaterialPath, WaypointPathColor);

            var units = new SelectableUnit[UnitColumns * UnitRows];
            for (int row = 0; row < UnitRows; row++)
            {
                for (int column = 0; column < UnitColumns; column++)
                {
                    float x = (column - (UnitColumns - 1) / 2f) * UnitSpacing;
                    float z = UnitStartForward + row * UnitSpacing;
                    int index = row * UnitColumns + column;
                    units[index] = CreateUnit($"{UnitName} {index + 1}", new Vector3(x, UnitHalfHeight, z), new UnitMaterials(body, marker, path));
                }
            }

            return units;
        }

        private static SelectableUnit CreateUnit(string unitName, Vector3 position, UnitMaterials materials)
        {
            GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unit.name = unitName;
            unit.transform.position = position;
            unit.GetComponent<Renderer>().sharedMaterial = materials.Body;

            // The agent stays off until the NavMesh is baked at startup; the unit enables it in Construct.
            var agent = unit.AddComponent<NavMeshAgent>();
            agent.speed = UnitAgentSpeed;
            agent.acceleration = UnitAgentAcceleration;

            // The capsule's pivot is its centre, so the agent lifts it by half its height to stand on the NavMesh.
            agent.baseOffset = UnitHalfHeight;
            agent.enabled = false;

            var selectable = unit.AddComponent<SelectableUnit>();
            var fields = new SerializedObject(selectable);
            fields.FindProperty(SelectionMarkerField).objectReferenceValue = CreateSelectionMarker(unit.transform, materials.Marker);
            fields.FindProperty(PathViewField).objectReferenceValue = CreateWaypointPath(unit.transform, materials.Path);
            fields.ApplyModifiedPropertiesWithoutUndo();

            unit.AddComponent<CombatUnit>();
            CreateMuzzle(unit.transform, LoadOrCreateUnlitMaterial(PlayerTracerMaterialPath, PlayerTracerColor));
            return selectable;
        }

        private static CombatUnit[] CreateEnemies()
        {
            var enemies = new CombatUnit[EnemyColumnsX.Length];
            for (int index = 0; index < enemies.Length; index++)
            {
                var position = new Vector3(EnemyColumnsX[index], UnitHalfHeight, EnemyRowZ);
                enemies[index] = CreateEnemy($"{EnemyName} {index + 1}", position);
            }

            return enemies;
        }

        /// <summary>An enemy is a unit without selection or orders, with an always-visible red ring so it reads as hostile.</summary>
        private static CombatUnit CreateEnemy(string enemyName, Vector3 position)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = enemyName;
            enemy.transform.position = position;
            enemy.GetComponent<Renderer>().sharedMaterial = LoadOrCreateLitMaterial(EnemyMaterialPath, EnemyColor);

            var agent = enemy.AddComponent<NavMeshAgent>();
            agent.speed = UnitAgentSpeed;
            agent.acceleration = UnitAgentAcceleration;
            agent.baseOffset = UnitHalfHeight;
            agent.enabled = false;

            Material marker = LoadOrCreateUnlitMaterial(EnemyMarkerMaterialPath, EnemyMarkerColor);
            CreateGroundRing(enemy.transform, marker, EnemyMarkerDiameter).name = EnemyMarkerName;
            CreateMuzzle(enemy.transform, LoadOrCreateUnlitMaterial(EnemyTracerMaterialPath, EnemyTracerColor));
            return enemy.AddComponent<CombatUnit>();
        }

        private static void CreateMuzzle(Transform unit, Material material)
        {
            var muzzle = new GameObject(MuzzleName);
            muzzle.transform.SetParent(unit, false);
            muzzle.transform.localPosition = new Vector3(0f, MuzzleHeightAboveCentre, 0f);

            var line = muzzle.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 0;
            line.widthMultiplier = TracerWidth;
            line.sharedMaterial = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var view = muzzle.AddComponent<HitEffectView>();
            var fields = new SerializedObject(view);
            fields.FindProperty(LineRendererField).objectReferenceValue = line;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignCombat(CombatInstaller installer, CombatUnit[] enemies)
        {
            var fields = new SerializedObject(installer);
            SerializedProperty enemyList = fields.FindProperty(EnemiesField);
            enemyList.arraySize = enemies.Length;
            for (int index = 0; index < enemies.Length; index++)
            {
                enemyList.GetArrayElementAtIndex(index).objectReferenceValue = enemies[index];
            }

            fields.FindProperty(PlayerSideField).FindPropertyRelative(SideWeaponField).objectReferenceValue = LoadOrCreateWeapon(PlayerWeaponPath, PlayerWeaponDamage);
            SerializedProperty enemySide = fields.FindProperty(EnemySideField);
            enemySide.FindPropertyRelative(SideWeaponField).objectReferenceValue = LoadOrCreateWeapon(EnemyWeaponPath, EnemyWeaponDamage);
            enemySide.FindPropertyRelative(SideMaxHealthField).floatValue = EnemyMaxHealth;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Only a newly created weapon gets <paramref name="damage"/>; an existing asset keeps its tuned values.</summary>
        private static WeaponDefinition LoadOrCreateWeapon(string path, float damage)
        {
            var existing = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (existing != null)
            {
                return existing;
            }

            EnsureFolder(WeaponsFolder);
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            AssetDatabase.CreateAsset(weapon, path);
            var fields = new SerializedObject(weapon);
            fields.FindProperty(WeaponDamageField).floatValue = damage;
            fields.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            return weapon;
        }

        private static GameObject CreateSelectionMarker(Transform unit, Material material)
        {
            GameObject marker = CreateGroundRing(unit, material, SelectionMarkerDiameter);
            marker.name = SelectionMarkerName;
            marker.SetActive(false);
            return marker;
        }

        private static GameObject CreateGroundRing(Transform unit, Material material, float diameter)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            // No collider: clicks must hit the unit's capsule, and the NavMesh bake must not see the ring.
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial = material;
            marker.transform.SetParent(unit, false);
            marker.transform.localPosition = new Vector3(0f, -UnitHalfHeight, 0f);
            marker.transform.localScale = new Vector3(diameter, SelectionMarkerThickness, diameter);
            return marker;
        }

        private static WaypointPathView CreateWaypointPath(Transform unit, Material material)
        {
            var pathObject = new GameObject(WaypointPathName);
            pathObject.transform.SetParent(unit, false);

            // The path view starts its line at its own position, so put it at the unit's feet.
            pathObject.transform.localPosition = new Vector3(0f, -UnitHalfHeight, 0f);

            var line = pathObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 0;
            line.widthMultiplier = WaypointPathWidth;
            line.sharedMaterial = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return pathObject.AddComponent<WaypointPathView>();
        }

        private static Material LoadOrCreateLitMaterial(string path, Color color)
        {
            return LoadOrCreateMaterial(path, () => HdrpMaterials.CreateLit(color));
        }

        private static Material LoadOrCreateUnlitMaterial(string path, Color color)
        {
            return LoadOrCreateMaterial(path, () => HdrpMaterials.CreateUnlit(color));
        }

        private static Material LoadOrCreateMaterial(string path, Func<Material> create)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = create();
            EnsureFolder(MaterialsFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static RtsCameraRig CreateCamera()
        {
            var cameraObject = new GameObject(CameraName) { tag = MainCameraTag };
            PhotorealSceneDefaults.ConfigureCamera(cameraObject.AddComponent<Camera>());
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, StartHeight, 0f);
            return cameraObject.AddComponent<RtsCameraRig>();
        }

        private static CombatInstaller CreateCombatInstaller(RtsCameraRig rig, SelectableUnit[] units)
        {
            var controls = new GameObject(ControlsName);
            var bootstrap = new GameObject(BootstrapName);
            var combatInstaller = bootstrap.AddComponent<CombatInstaller>();

            var fields = new SerializedObject(combatInstaller);
            fields.FindProperty(CameraRigField).objectReferenceValue = rig;
            fields.FindProperty(CameraSettingsField).objectReferenceValue = LoadOrCreateCameraSettings();
            fields.FindProperty(CameraField).objectReferenceValue = rig.GetComponent<Camera>();
            fields.FindProperty(SelectionControllerField).objectReferenceValue = controls.AddComponent<SelectionController>();
            fields.FindProperty(SelectionBoxViewField).objectReferenceValue = controls.AddComponent<SelectionBoxView>();
            fields.FindProperty(MoveOrderControllerField).objectReferenceValue = controls.AddComponent<MoveOrderController>();
            fields.FindProperty(FormationLabelViewField).objectReferenceValue = controls.AddComponent<FormationLabelView>();
            fields.FindProperty(StanceControllerField).objectReferenceValue = controls.AddComponent<StanceController>();
            fields.FindProperty(StanceLabelViewField).objectReferenceValue = controls.AddComponent<StanceLabelView>();

            SerializedProperty unitList = fields.FindProperty(UnitsField);
            unitList.arraySize = units.Length;
            for (int index = 0; index < units.Length; index++)
            {
                unitList.GetArrayElementAtIndex(index).objectReferenceValue = units[index];
            }

            fields.ApplyModifiedPropertiesWithoutUndo();
            return combatInstaller;
        }

        /// <summary>The ground installer comes first: it bakes the NavMesh that the combat installer's units are placed on.</summary>
        private static void CreateBootstrapper(GroundNavMeshInstaller groundInstaller, CombatInstaller combatInstaller)
        {
            var bootstrapper = combatInstaller.gameObject.AddComponent<GameBootstrapper>();
            var fields = new SerializedObject(bootstrapper);
            SerializedProperty installers = fields.FindProperty(InstallersField);
            installers.arraySize = 2;
            installers.GetArrayElementAtIndex(0).objectReferenceValue = groundInstaller;
            installers.GetArrayElementAtIndex(1).objectReferenceValue = combatInstaller;
            fields.ApplyModifiedPropertiesWithoutUndo();
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
