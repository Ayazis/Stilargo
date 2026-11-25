using System;
using System.Linq;
using Stilargo.Core.Hill;
using Stilargo.Editor.Rendering;
using Stilargo.Editor.World;
using Stilargo.Gameplay.Combat;
using Stilargo.Gameplay.Composition;
using Stilargo.Gameplay.Fortification;
using Stilargo.Gameplay.Hill;
using Stilargo.Gameplay.Units;
using Stilargo.Gameplay.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Stilargo.Editor.Hill
{
    /// <summary>
    /// Adds step 10 to a scene: the hill-and-rival installer, the hill marker disc, the hill HUD and the match
    /// loop. The marker is placed from <c>Hill.json</c> at startup, so only its looks are set here.
    /// </summary>
    public static class HillSceneSetup
    {
        private const string HolderName = "Hill And Rival";
        private const string MarkerName = "Hill Marker";
        private const string MaterialsFolder = "Assets/_Project/Materials";
        private const string MaterialPathFormat = MaterialsFolder + "/Greybox_Hill{0}.mat";

        private const string WorldField = "_world";
        private const string CombatField = "_combat";
        private const string FortificationsField = "_fortifications";
        private const string PlayerUnitsField = "_playerUnits";
        private const string EnemiesField = "_enemies";
        private const string CombatUnitsField = "_units";
        private const string ZoneViewField = "_zoneView";
        private const string HudViewField = "_hudView";
        private const string MatchLoopField = "_matchLoop";
        private const string EndPauseField = "_endPause";
        private const string MarkerField = "_marker";
        private const string LooksField = "_looks";
        private const string LookStateField = "_state";
        private const string LookMaterialField = "_material";

        // Calm grey when free, the squad's blue, the enemy's red and a warning amber; all inside the PBR albedo range.
        private static readonly Color EmptyColor = new Color(150f / 255f, 150f / 255f, 150f / 255f);
        private static readonly Color PlayerColor = new Color(70f / 255f, 130f / 255f, 220f / 255f);
        private static readonly Color RivalColor = new Color(200f / 255f, 60f / 255f, 50f / 255f);
        private static readonly Color ContestedColor = new Color(225f / 255f, 170f / 255f, 60f / 255f);

        public static HillRivalInstaller Create(WorldInstaller world, CombatInstaller combat, FortificationInstaller fortifications)
        {
            var holder = new GameObject(HolderName);
            var installer = holder.AddComponent<HillRivalInstaller>();
            SerializedFields.Assign(installer, WorldField, world);
            SerializedFields.Assign(installer, CombatField, combat);
            SerializedFields.Assign(installer, FortificationsField, fortifications);
            SerializedFields.AssignArray(installer, PlayerUnitsField, PlayerCombatUnits(combat));
            SerializedFields.AssignArray(installer, EnemiesField, ReadArray(combat, EnemiesField));
            SerializedFields.Assign(installer, ZoneViewField, CreateZoneView(holder.transform));
            SerializedFields.Assign(installer, HudViewField, holder.AddComponent<HillHudView>());
            SerializedFields.Assign(installer, MatchLoopField, holder.AddComponent<MatchLoop>());
            SerializedFields.Assign(installer, EndPauseField, holder.AddComponent<MatchEndPause>());
            return installer;
        }

        /// <summary>Enemies are the rival's units, so each needs an <see cref="AutonomousUnit"/> to take its orders.</summary>
        public static void MakeAutonomous(CombatUnit enemy)
        {
            enemy.gameObject.AddComponent<AutonomousUnit>();
        }

        private static UnityEngine.Object[] PlayerCombatUnits(CombatInstaller combat)
        {
            return ReadArray(combat, CombatUnitsField)
                .Select(unit => (UnityEngine.Object)((Component)unit).GetComponent<CombatUnit>())
                .ToArray();
        }

        private static UnityEngine.Object[] ReadArray(UnityEngine.Object owner, string field)
        {
            SerializedProperty property = new SerializedObject(owner).FindProperty(field);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException($"{owner.GetType().Name} has no serialized array '{field}'.");
            }

            var values = new UnityEngine.Object[property.arraySize];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = property.GetArrayElementAtIndex(index).objectReferenceValue;
            }

            return values;
        }

        private static HillZoneView CreateZoneView(Transform parent)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = MarkerName;

            // No collider: clicks must reach the road below, and the NavMesh bake must not see the disc.
            UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.SetParent(parent, false);
            Renderer renderer = marker.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var view = parent.gameObject.AddComponent<HillZoneView>();
            SerializedFields.Assign(view, MarkerField, renderer);
            AssignLooks(view);
            renderer.sharedMaterial = LoadOrCreateMaterial(HillState.Empty, EmptyColor);
            return view;
        }

        private static void AssignLooks(HillZoneView view)
        {
            var fields = new SerializedObject(view);
            SerializedProperty looks = fields.FindProperty(LooksField);
            looks.arraySize = 0;
            AddLook(looks, HillState.Empty, EmptyColor);
            AddLook(looks, HillState.HeldByPlayer, PlayerColor);
            AddLook(looks, HillState.HeldByRival, RivalColor);
            AddLook(looks, HillState.Contested, ContestedColor);
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddLook(SerializedProperty looks, HillState state, Color color)
        {
            looks.arraySize++;
            SerializedProperty look = looks.GetArrayElementAtIndex(looks.arraySize - 1);
            look.FindPropertyRelative(LookStateField).enumValueIndex = (int)state;
            look.FindPropertyRelative(LookMaterialField).objectReferenceValue = LoadOrCreateMaterial(state, color);
        }

        private static Material LoadOrCreateMaterial(HillState state, Color color)
        {
            string path = string.Format(MaterialPathFormat, state);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Material material = HdrpMaterials.CreateUnlit(color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
