using System;
using System.Linq;
using Stilargo.Gameplay.CameraControl;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Cover;
using Stilargo.Gameplay.Garrison;
using Stilargo.Gameplay.GarrisonViews;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Economy;
using Stilargo.Gameplay.Fog;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.Orders;
using Stilargo.Gameplay.Scavenging;
using Stilargo.Gameplay.Selection;
using Stilargo.Gameplay.Stances;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using Stilargo.Editor.Fortification;
using Stilargo.Editor.Hill;
using Stilargo.Editor.Rendering;
using Stilargo.Editor.World;
using Stilargo.Infrastructure.Composition;
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
        private const string UnitMaterialPath = MaterialsFolder + "/Greybox_Unit.mat";
        private const string SelectionMarkerMaterialPath = MaterialsFolder + "/Greybox_SelectionMarker.mat";
        private const string WaypointPathMaterialPath = MaterialsFolder + "/Greybox_WaypointPath.mat";

        // Distinct colours so units and their markers stand out from the city; Unity's default material is plain white.
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
        private const string CoverMaterialPath = MaterialsFolder + "/Greybox_Cover.mat";
        private const string GarrisonSlotMaterialPath = MaterialsFolder + "/Greybox_GarrisonSlot.mat";
        private const string CoverGarrisonName = "Cover And Garrisons";
        private const string CoverPointViewField = "_coverPointView";
        private const string SlotMarkerViewField = "_slotMarkerView";
        private const string ViewMaterialField = "_material";

        // Muted sandbag burlap and a calm teal marker; both inside the PBR albedo range.
        private static readonly Color CoverColor = new Color(138f / 255f, 125f / 255f, 99f / 255f);
        private static readonly Color GarrisonSlotColor = new Color(63f / 255f, 167f / 255f, 176f / 255f);

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

        // Enemies stand in a row on the next street north of home: out of vision at the start, one block away.
        private static readonly float[] EnemyOffsetsX = { -6f, -2f, 2f, 6f };
        private const float EnemyOffsetZ = 28f;
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
        private const string UnitWorldName = "Unit World Links";
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
        private const string WorldField = "_world";
        private const string VisionSourcesField = "_visionSources";
        private const string ScavengersField = "_scavengers";
        private const string FogHideablesField = "_fogHideables";
        private const string WalletLabelViewField = "_walletLabelView";
        private const string CameraBoundsMinField = "_boundsMin";
        private const string CameraBoundsMaxField = "_boundsMax";

        private const float StartHeight = 30f;
        private const float SunPitchDegrees = 50f;
        private const float SunYawDegrees = -30f;

        // A small squad in a loose grid on the street through home. The roads are 4 m wide, so rows stay within ±1 m.
        private const int UnitColumns = 3;
        private const int UnitRows = 2;
        private const float UnitColumnSpacing = 3f;
        private const float UnitRowSpacing = 2f;
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
            WorldInstaller world = CreateWorld();
            CitySite site = CitySite.Of(world);
            SelectableUnit[] units = CreateUnits(site);
            CombatUnit[] enemies = CreateEnemies(site);
            RtsCameraRig rig = CreateCamera(site);
            CombatInstaller combatInstaller = CreateCombatInstaller(rig, units, world);
            AssignCombat(combatInstaller, enemies);
            CoverGarrisonInstaller coverGarrisonInstaller = CreateCoverGarrisonInstaller(combatInstaller, world, enemies);
            UnitWorldInstaller unitWorldInstaller = CreateUnitWorldInstaller(world, units, enemies);
            FortificationInstaller fortificationInstaller = FortificationSceneSetup.Create(world, coverGarrisonInstaller, enemies);
            Installer hillRivalInstaller = HillSceneSetup.Create(world, combatInstaller, fortificationInstaller);
            CreateBootstrapper(combatInstaller.gameObject, new Installer[] { world, combatInstaller, coverGarrisonInstaller, unitWorldInstaller, fortificationInstaller, hillRivalInstaller });

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
        /// FL's World prefab: it generates the city, bakes its NavMesh and sets up fog and loot at startup.
        /// It stays a prefab instance, so rebuilding the prefab updates Main without touching this scene.
        /// </summary>
        private static WorldInstaller CreateWorld()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorldPrefabBuilder.PrefabPath);
            if (prefab == null)
            {
                prefab = WorldPrefabBuilder.Build();
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            WorldInstaller world = instance.GetComponent<WorldInstaller>();
            if (world == null)
            {
                throw new InvalidOperationException($"{WorldPrefabBuilder.PrefabPath} has no {nameof(WorldInstaller)}.");
            }

            return world;
        }

        private static SelectableUnit[] CreateUnits(CitySite site)
        {
            Material body = LoadOrCreateLitMaterial(UnitMaterialPath, UnitColor);
            Material marker = LoadOrCreateUnlitMaterial(SelectionMarkerMaterialPath, SelectionMarkerColor);
            Material path = LoadOrCreateUnlitMaterial(WaypointPathMaterialPath, WaypointPathColor);

            var units = new SelectableUnit[UnitColumns * UnitRows];
            for (int row = 0; row < UnitRows; row++)
            {
                for (int column = 0; column < UnitColumns; column++)
                {
                    float x = (column - (UnitColumns - 1) / 2f) * UnitColumnSpacing;
                    float z = (row - (UnitRows - 1) / 2f) * UnitRowSpacing;
                    Vector3 position = site.RoadPointNearHome(new Vector3(x, 0f, z)) + Vector3.up * UnitHalfHeight;
                    int index = row * UnitColumns + column;
                    units[index] = CreateUnit($"{UnitName} {index + 1}", position, new UnitMaterials(body, marker, path));
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
            unit.AddComponent<VisionSourceComponent>();
            unit.AddComponent<ScavengerComponent>();
            unit.AddComponent<GarrisonOccupant>();
            CreateMuzzle(unit.transform, LoadOrCreateUnlitMaterial(PlayerTracerMaterialPath, PlayerTracerColor));
            return selectable;
        }

        private static CombatUnit[] CreateEnemies(CitySite site)
        {
            var enemies = new CombatUnit[EnemyOffsetsX.Length];
            for (int index = 0; index < enemies.Length; index++)
            {
                var offset = new Vector3(EnemyOffsetsX[index], 0f, EnemyOffsetZ);
                Vector3 position = site.RoadPointNearHome(offset) + Vector3.up * UnitHalfHeight;
                enemies[index] = CreateEnemy($"{EnemyName} {index + 1}", position);
            }

            return enemies;
        }

        /// <summary>An enemy is a unit without selection or orders, with a red ring so it reads as hostile. The fog hides it unless seen.</summary>
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
            enemy.AddComponent<FogHideable>();
            var combatUnit = enemy.AddComponent<CombatUnit>();
            HillSceneSetup.MakeAutonomous(combatUnit);
            return combatUnit;
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

        /// <summary>
        /// The rig works out its focus from where the camera stands, so start it behind home at the configured
        /// pitch to look straight at the squad.
        /// </summary>
        private static RtsCameraRig CreateCamera(CitySite site)
        {
            CameraSettings settings = LoadOrCreateCameraSettings();
            FitCameraBoundsToCity(settings, site);

            var cameraObject = new GameObject(CameraName) { tag = MainCameraTag };
            PhotorealSceneDefaults.ConfigureCamera(cameraObject.AddComponent<Camera>());
            cameraObject.AddComponent<AudioListener>();
            float pitchRadians = settings.PitchDegrees * Mathf.Deg2Rad;
            float behindFocus = StartHeight * Mathf.Cos(pitchRadians) / Mathf.Sin(pitchRadians);
            cameraObject.transform.position = site.Home + new Vector3(0f, StartHeight, -behindFocus);
            return cameraObject.AddComponent<RtsCameraRig>();
        }

        /// <summary>The camera may pan over the whole city and no further. Only the bounds are written; other tuning stays.</summary>
        private static void FitCameraBoundsToCity(CameraSettings settings, CitySite site)
        {
            var fields = new SerializedObject(settings);
            fields.FindProperty(CameraBoundsMinField).vector2Value = Vector2.zero;
            fields.FindProperty(CameraBoundsMaxField).vector2Value = site.Size;
            fields.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        private static UnitWorldInstaller CreateUnitWorldInstaller(WorldInstaller world, SelectableUnit[] units, CombatUnit[] enemies)
        {
            var links = new GameObject(UnitWorldName);
            var installer = links.AddComponent<UnitWorldInstaller>();
            SerializedFields.Assign(installer, WorldField, world);
            SerializedFields.AssignArray(installer, VisionSourcesField, units.Select(unit => unit.GetComponent<VisionSourceComponent>()).ToArray());
            SerializedFields.AssignArray(installer, ScavengersField, units.Select(unit => unit.GetComponent<ScavengerComponent>()).ToArray());
            SerializedFields.AssignArray(installer, FogHideablesField, enemies.Select(enemy => enemy.GetComponent<FogHideable>()).ToArray());
            SerializedFields.Assign(installer, WalletLabelViewField, links.AddComponent<WalletLabelView>());
            return installer;
        }

        private static CombatInstaller CreateCombatInstaller(RtsCameraRig rig, SelectableUnit[] units, WorldInstaller world)
        {
            var controls = new GameObject(ControlsName);
            var bootstrap = new GameObject(BootstrapName);
            var combatInstaller = bootstrap.AddComponent<CombatInstaller>();

            var fields = new SerializedObject(combatInstaller);
            fields.FindProperty(WorldField).objectReferenceValue = world;
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

        /// <summary>Cover points and garrison slots come from the city at startup; this only adds their installer and views.</summary>
        private static CoverGarrisonInstaller CreateCoverGarrisonInstaller(CombatInstaller combatInstaller, WorldInstaller world, CombatUnit[] enemies)
        {
            var combatFields = new SerializedObject(combatInstaller);
            var holder = new GameObject(CoverGarrisonName);
            var installer = holder.AddComponent<CoverGarrisonInstaller>();
            var coverView = holder.AddComponent<CoverPointView>();
            var slotView = holder.AddComponent<GarrisonSlotMarkerView>();
            SerializedFields.Assign(coverView, ViewMaterialField, LoadOrCreateLitMaterial(CoverMaterialPath, CoverColor));
            SerializedFields.Assign(slotView, ViewMaterialField, LoadOrCreateUnlitMaterial(GarrisonSlotMaterialPath, GarrisonSlotColor));

            SerializedFields.Assign(installer, WorldField, world);
            SerializedFields.Assign(installer, MoveOrderControllerField, combatFields.FindProperty(MoveOrderControllerField).objectReferenceValue);
            SerializedProperty units = combatFields.FindProperty(UnitsField);
            var unitObjects = new UnityEngine.Object[units.arraySize];
            for (int index = 0; index < unitObjects.Length; index++)
            {
                unitObjects[index] = units.GetArrayElementAtIndex(index).objectReferenceValue;
            }

            SerializedFields.AssignArray(installer, UnitsField, unitObjects);
            SerializedFields.AssignArray(installer, EnemiesField, enemies);
            SerializedFields.Assign(installer, CoverPointViewField, coverView);
            SerializedFields.Assign(installer, SlotMarkerViewField, slotView);
            return installer;
        }

        /// <summary>
        /// Runs in list order. The world comes first: it bakes the NavMesh the combat installer's units are placed on,
        /// and creates the fog and loot that the unit-world links hand to those units.
        /// </summary>
        private static void CreateBootstrapper(GameObject bootstrap, Installer[] installers)
        {
            var bootstrapper = bootstrap.AddComponent<GameBootstrapper>();
            SerializedFields.AssignArray(bootstrapper, InstallersField, installers);
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

        /// <summary>
        /// Puts Main first in Build Settings, because the first scene is the one the exe loads at startup.
        /// Entries for deleted scenes are dropped, since a build fails on them.
        /// </summary>
        private static void AddToBuildSettings()
        {
            EditorBuildSettingsScene[] others = EditorBuildSettings.scenes
                .Where(buildScene => buildScene.path != ScenePath)
                .Where(buildScene => AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path) != null)
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
