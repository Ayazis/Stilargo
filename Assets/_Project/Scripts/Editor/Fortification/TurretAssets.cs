using System;
using System.Linq;
using Stilargo.Editor.Rendering;
using Stilargo.Editor.World;
using Stilargo.Gameplay.Fortification;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Fortification
{
    /// <summary>
    /// Builds the turret prefab from the Sketchfab model that <c>Tools/Art/prepare_turret.py</c> exports: an HDRP
    /// material from its texture set, and a <see cref="TurretRigView"/> wired to its Yaw and Pitch bones and Muzzle point.
    /// </summary>
    public static class TurretAssets
    {
        private const string ModelFolder = "Assets/_Project/Art/Models/Turret";
        private const string ModelPath = ModelFolder + "/Turret.fbx";
        private const string TextureFolder = ModelFolder + "/Textures";
        private const string TextureName = "Turret";
        private const string MaterialPath = ModelFolder + "/Turret.mat";
        private const string PrefabPath = "Assets/_Project/World/Turret.prefab";
        private const string YawBone = "Yaw";
        private const string PitchBone = "Pitch";
        private const string MuzzlePoint = "Muzzle";
        private const string YawField = "_yaw";
        private const string PitchField = "_pitch";
        private const string MuzzleField = "_muzzle";
        private const string BarrelsField = "_barrels";
        private const string FlashField = "_flash";
        private const string DeckHalfSizeField = "_halfSizeMetres";

        // The textures carry the colour; a white tint leaves them as authored.
        private static readonly Color NoTint = Color.white;

        public static TurretRigView LoadOrCreatePrefab()
        {
            ConfigureImport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = TextureName;
            try
            {
                StripAnimator(instance);
                AssignMaterial(instance);
                Transform pitch = FindChild(instance.transform, PitchBone);
                Transform muzzle = FindChild(instance.transform, MuzzlePoint);
                var rig = instance.AddComponent<TurretRigView>();
                SerializedFields.Assign(rig, YawField, FindChild(instance.transform, YawBone));
                SerializedFields.Assign(rig, PitchField, pitch);
                SerializedFields.Assign(rig, MuzzleField, muzzle);
                AddShotEffects(instance, pitch, muzzle);
                AddDeck(instance);
                return PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath).GetComponent<TurretRigView>();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void AddShotEffects(GameObject instance, Transform pitch, Transform muzzle)
        {
            GameObject flash = TurretPrefabParts.AddFlash(muzzle, muzzle.position - pitch.position);
            var effects = instance.GetComponent<TurretShotEffects>();
            SerializedFields.Assign(effects, BarrelsField, pitch);
            SerializedFields.Assign(effects, MuzzleField, muzzle);
            SerializedFields.Assign(effects, FlashField, flash);
        }

        private static void AddDeck(GameObject instance)
        {
            TurretPrefabParts.AddDeck(instance.transform);
            var deck = instance.AddComponent<TurretDeck>();
            var fields = new SerializedObject(deck);
            fields.FindProperty(DeckHalfSizeField).floatValue = TurretPrefabParts.DeckHalfSizeMetres;
            fields.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureImport()
        {
            if (!(AssetImporter.GetAtPath(ModelPath) is ModelImporter importer))
            {
                throw new InvalidOperationException($"No model at {ModelPath}. Run Tools/Art/prepare_turret.py in Blender.");
            }

            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = false;
            importer.addCollider = false;
            importer.SaveAndReimport();
        }

        // Bones are posed from code; an Animator without a controller would only cost time each frame.
        private static void StripAnimator(GameObject instance)
        {
            var animator = instance.GetComponent<Animator>();
            if (animator != null)
            {
                UnityEngine.Object.DestroyImmediate(animator);
            }
        }

        private static void AssignMaterial(GameObject instance)
        {
            Material material = LoadOrCreateMaterial();
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Material LoadOrCreateMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = HdrpMaterials.CreateLit(NoTint);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            HdrpMaterials.ApplyUvTextures(material, SurfaceTextures.LoadPng(TextureFolder, TextureName));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform FindChild(Transform root, string childName)
        {
            Transform found = root.GetComponentsInChildren<Transform>().FirstOrDefault(child => child.name == childName);
            if (found == null)
            {
                throw new InvalidOperationException($"{ModelPath} has no '{childName}'. Re-run Tools/Art/prepare_turret.py.");
            }

            return found;
        }
    }
}
