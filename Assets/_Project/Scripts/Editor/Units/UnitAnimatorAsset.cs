using System;
using System.IO;
using System.Linq;
using Stilargo.Core.Animation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Stilargo.Editor.Units
{
    /// <summary>
    /// Builds the unit Animator Controller from the Mixamo rifle clips under <c>Art/Animations/Mixamo/</c>.
    /// Clips import as Humanoid, so they retarget onto any humanoid avatar. One state per <see cref="UnitPose"/>,
    /// named after it; <c>UnitAnimationView</c> cross-fades between them, so the controller needs no transitions.
    /// </summary>
    public static class UnitAnimatorAsset
    {
        private const string ClipFolder = "Assets/_Project/Art/Animations/Mixamo";
        private const string ControllerPath = "Assets/_Project/Units/UnitAnimator.controller";

        public static AnimatorController LoadOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (existing != null)
            {
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorStateMachine states = controller.layers[0].stateMachine;
            foreach (UnitPose pose in Enum.GetValues(typeof(UnitPose)))
            {
                AnimatorState state = states.AddState(pose.ToString());
                state.motion = ImportLoopingClip(ClipFileFor(pose));
            }

            states.defaultState = states.states.First(child => child.state.name == UnitPose.Idle.ToString()).state;
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static string ClipFileFor(UnitPose pose)
        {
            switch (pose)
            {
                case UnitPose.Idle:
                    return "RifleIdle";
                case UnitPose.Run:
                    return "RifleRun";
                case UnitPose.Aim:
                    return "RifleAim";
                case UnitPose.Crouch:
                    return "RifleCrouch";
                default:
                    throw new ArgumentOutOfRangeException(nameof(pose), pose, "No clip for this pose.");
            }
        }

        private static AnimationClip ImportLoopingClip(string fileName)
        {
            string path = $"{ClipFolder}/{fileName}.fbx";
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                throw new InvalidOperationException($"No animation at {path}.");
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.loopTime = true;
                // Units move by NavMeshAgent; the clip must not drift or turn the body.
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();

            AnimationClip result = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(asset => !asset.name.StartsWith("__preview__", StringComparison.Ordinal));
            if (result == null)
            {
                throw new InvalidOperationException($"{path} has no animation clip.");
            }

            return result;
        }
    }
}
