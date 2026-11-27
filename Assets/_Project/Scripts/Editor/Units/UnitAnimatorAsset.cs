using System;
using System.IO;
using System.Linq;
using Stilargo.Core.Animation;
using Stilargo.Gameplay.Animation;
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
                RefreshClips(existing);
                DriveRunSpeed(existing);
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
            DriveRunSpeed(controller);
            return controller;
        }

        /// <summary>The run clip as imported, for measuring how fast its feet move.</summary>
        public static AnimationClip LoadRunClip()
        {
            string path = $"{ClipFolder}/{ClipFileFor(UnitPose.Run)}.fbx";
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(asset => !asset.name.StartsWith("__preview__", StringComparison.Ordinal));
            if (clip == null)
            {
                throw new InvalidOperationException($"{path} has no animation clip.");
            }

            return clip;
        }

        // Reapplies the import settings to every clip, so a controller made before they changed picks them up.
        private static void RefreshClips(AnimatorController controller)
        {
            foreach (ChildAnimatorState child in controller.layers[0].stateMachine.states)
            {
                if (Enum.TryParse(child.state.name, out UnitPose pose))
                {
                    child.state.motion = ImportLoopingClip(ClipFileFor(pose));
                }
            }
        }

        // The run state plays at the speed the view sets, so the feet keep pace with the NavMeshAgent. Safe to rerun.
        private static void DriveRunSpeed(AnimatorController controller)
        {
            if (controller.parameters.All(parameter => parameter.name != UnitAnimationView.RunSpeedParameter))
            {
                controller.AddParameter(new AnimatorControllerParameter
                {
                    name = UnitAnimationView.RunSpeedParameter,
                    type = AnimatorControllerParameterType.Float,
                    defaultFloat = 1f,
                });
            }

            AnimatorState run = controller.layers[0].stateMachine.states
                .Select(child => child.state)
                .First(state => state.name == UnitPose.Run.ToString());
            run.speedParameterActive = true;
            run.speedParameter = UnitAnimationView.RunSpeedParameter;
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
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

                // Units move by NavMeshAgent; the clip must not turn or lift the body. Forward motion stays root
                // motion, which the Animator discards (applyRootMotion is off): baked into the pose, the run would
                // carry the body a couple of metres ahead of the unit each cycle and snap it back.
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = false;
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
