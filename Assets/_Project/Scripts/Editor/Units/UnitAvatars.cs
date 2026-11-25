using System;
using System.IO;
using System.Linq;
using Stilargo.Editor.Rendering;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Units
{
    /// <summary>
    /// Turns the Rocketbox military avatars under <c>Art/Models/Units/&lt;name&gt;/</c> into HDRP prefabs.
    /// Each FBX material <c>&lt;m&gt;</c> is remapped to an HDRP/Lit material built from
    /// <c>Textures/&lt;m&gt;_albedo.png</c>, <c>_normal.png</c> and <c>_mask.png</c>.
    /// </summary>
    public static class UnitAvatars
    {
        public const string PlayerAvatarA = "Military_Male_01";
        public const string PlayerAvatarB = "Military_Male_03";
        public const string PlayerAvatarC = "Military_Female_01";
        public const string EnemyAvatar = "Military_Male_02";

        private const string ArtRoot = "Assets/_Project/Art/Models/Units";
        private const string PrefabFolder = "Assets/_Project/Units";

        // Average soldier height; the model is scaled to it so its source units don't matter.
        private const float StandingHeightMetres = 1.8f;

        /// <summary>Loads the avatar prefab, building it from the imported FBX when missing.</summary>
        public static GameObject LoadOrCreate(string avatarName)
        {
            string prefabPath = $"{PrefabFolder}/{avatarName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                return existing;
            }

            GameObject model = ImportModel(avatarName);
            Directory.CreateDirectory(PrefabFolder);
            // The model sits under a plain root, so its scale and feet offset survive placing the prefab anywhere.
            var root = new GameObject(avatarName);
            try
            {
                var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                FitToStandingHeight(modelInstance);
                AttachAnimator(modelInstance);
                return PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject ImportModel(string avatarName)
        {
            string folder = $"{ArtRoot}/{avatarName}";
            string modelPath = $"{folder}/{avatarName}.fbx";
            if (!(AssetImporter.GetAtPath(modelPath) is ModelImporter importer))
            {
                throw new InvalidOperationException($"No avatar model at {modelPath}.");
            }

            foreach (string materialName in SourceMaterialNames(modelPath))
            {
                Material material = LoadOrCreateMaterial(folder, materialName);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), materialName), material);
            }

            // Humanoid, so the Mixamo rifle clips retarget onto the Rocketbox skeleton.
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        private static void AttachAnimator(GameObject model)
        {
            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{model.name} imported without an Animator; is it a Humanoid model?");
            }

            animator.runtimeAnimatorController = UnitAnimatorAsset.LoadOrCreate();
            // The NavMeshAgent moves the unit; root motion from the clips would fight it.
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }

        private static string[] SourceMaterialNames(string modelPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<Renderer>()
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct()
                .ToArray();
        }

        private static Material LoadOrCreateMaterial(string folder, string materialName)
        {
            string materialFolder = $"{folder}/Materials";
            string path = $"{materialFolder}/{materialName}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(materialFolder);
            Material material = HdrpMaterials.CreateLit(Color.white);
            HdrpMaterials.ApplyUvTextures(material, SurfaceTextures.LoadPng($"{folder}/Textures", materialName));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void FitToStandingHeight(GameObject avatar)
        {
            Bounds bounds = ActiveBounds(avatar);
            float scale = StandingHeightMetres / bounds.size.y;
            avatar.transform.localScale *= scale;

            // Feet on the pivot, so placing the avatar at ground height stands it on the ground.
            Bounds scaled = ActiveBounds(avatar);
            avatar.transform.localPosition -= new Vector3(0f, scaled.min.y, 0f);
        }

        private static Bounds ActiveBounds(GameObject avatar)
        {
            Renderer[] active = avatar.GetComponentsInChildren<Renderer>(false);
            Bounds bounds = active[0].bounds;
            foreach (Renderer renderer in active.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}
