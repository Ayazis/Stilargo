using System;
using Stilargo.Core.Fortification;
using Stilargo.Editor.Rendering;
using Stilargo.Editor.World;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stilargo.Editor.Fortification
{
    /// <summary>
    /// Adds FL's claim-and-fortify objects to a scene: the installer, the turret runner, the greybox views and
    /// the placeholder panel. The Main scene creator calls this so the scene stays generated from code.
    /// </summary>
    public static class FortificationSceneSetup
    {
        private const string HolderName = "Fortifications";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string BarricadeMaterialPath = MaterialsFolder + "/Greybox_Barricade.mat";
        private const string WindowMaterialPath = MaterialsFolder + "/Greybox_ReinforcedWindow.mat";
        private const string TracerMaterialPath = MaterialsFolder + "/Greybox_TurretTracer.mat";
        private const string OutlineMaterialPath = MaterialsFolder + "/Greybox_BuildingOutline.mat";
        private const float TracerWidthMetres = 0.06f;

        // Weathered timber and galvanised steel plate; both inside the PBR albedo range.
        private static readonly Color BarricadeColor = new Color(120f / 255f, 92f / 255f, 64f / 255f);
        private static readonly Color WindowColor = new Color(128f / 255f, 132f / 255f, 136f / 255f);
        private static readonly Color TracerColor = new Color(240f / 255f, 200f / 255f, 120f / 255f);

        // The same bright green as the ring under a selected unit, so a selected building reads the same way.
        private static readonly Color OutlineColor = new Color(0.3f, 1f, 0.4f);

        // A door-wide barricade and a plate over a window. Roof turrets use the turret model instead of a block.
        private static readonly Vector3 BarricadeSizeMetres = new Vector3(2.4f, 1.4f, 0.5f);
        private static readonly Vector3 WindowSizeMetres = new Vector3(1.2f, 1.2f, 0.15f);

        private const string WorldField = "_world";
        private const string GarrisonsField = "_garrisons";
        private const string CombatField = "_combat";
        private const string OutlineMaterialField = "_selectionOutlineMaterial";
        private const string EnemiesField = "_enemies";
        private const string TurretBatteryField = "_turretBattery";
        private const string TracerViewField = "_tracerView";
        private const string TurretModelField = "_modelPrefab";
        private const string FortificationViewField = "_fortificationView";
        private const string LooksField = "_looks";
        private const string LookSlotKindField = "_slotKind";
        private const string LookMaterialField = "_material";
        private const string LookSizeField = "_sizeMetres";

        /// <summary>The enemies the turrets shoot are read from the combat installer, so both lists always match.</summary>
        public static FortificationInstaller Create(WorldInstaller world, CombatInstaller combat, CoverGarrisonInstaller garrisons)
        {
            var holder = new GameObject(HolderName);
            var installer = holder.AddComponent<FortificationInstaller>();
            var fortificationView = holder.AddComponent<FortificationView>();
            AssignLooks(fortificationView);

            SerializedFields.Assign(installer, WorldField, world);
            SerializedFields.Assign(installer, GarrisonsField, garrisons);
            SerializedFields.Assign(installer, CombatField, combat);
            SerializedFields.Assign(installer, OutlineMaterialField, LoadOrCreateMaterial(OutlineMaterialPath, () => HdrpMaterials.CreateUnlit(OutlineColor)));
            SerializedFields.AssignArray(installer, EnemiesField, EnemiesOf(combat));
            SerializedFields.Assign(installer, TurretBatteryField, CreateTurretBattery(holder));
            SerializedFields.Assign(installer, TracerViewField, CreateTracerView(holder.transform));
            SerializedFields.Assign(installer, FortificationViewField, fortificationView);
            return installer;
        }

        private static UnityEngine.Object[] EnemiesOf(CombatInstaller combat)
        {
            SerializedProperty enemies = new SerializedObject(combat).FindProperty(EnemiesField);
            var result = new UnityEngine.Object[enemies.arraySize];
            for (int index = 0; index < result.Length; index++)
            {
                result[index] = enemies.GetArrayElementAtIndex(index).objectReferenceValue;
            }

            return result;
        }

        private static TurretBattery CreateTurretBattery(GameObject holder)
        {
            var battery = holder.AddComponent<TurretBattery>();
            SerializedFields.Assign(battery, TurretModelField, TurretAssets.LoadOrCreatePrefab());
            return battery;
        }

        private static TurretTracerView CreateTracerView(Transform parent)
        {
            var tracer = new GameObject("Turret Tracer");
            tracer.transform.SetParent(parent, false);
            var line = tracer.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = TracerWidthMetres;
            line.sharedMaterial = LoadOrCreateMaterial(TracerMaterialPath, () => HdrpMaterials.CreateUnlit(TracerColor));
            line.shadowCastingMode = ShadowCastingMode.Off;
            return tracer.AddComponent<TurretTracerView>();
        }

        private static void AssignLooks(FortificationView view)
        {
            var fields = new SerializedObject(view);
            SerializedProperty looks = fields.FindProperty(LooksField);
            looks.arraySize = 0;
            AddLook(looks, FortificationSlotKind.Entrance, LoadOrCreateMaterial(BarricadeMaterialPath, () => HdrpMaterials.CreateLit(BarricadeColor)), BarricadeSizeMetres);
            AddLook(looks, FortificationSlotKind.Window, LoadOrCreateMaterial(WindowMaterialPath, () => HdrpMaterials.CreateLit(WindowColor)), WindowSizeMetres);
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddLook(SerializedProperty looks, FortificationSlotKind kind, Material material, Vector3 size)
        {
            looks.arraySize++;
            SerializedProperty look = looks.GetArrayElementAtIndex(looks.arraySize - 1);
            look.FindPropertyRelative(LookSlotKindField).enumValueIndex = (int)kind;
            look.FindPropertyRelative(LookMaterialField).objectReferenceValue = material;
            look.FindPropertyRelative(LookSizeField).vector3Value = size;
        }

        private static Material LoadOrCreateMaterial(string path, Func<Material> create)
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
