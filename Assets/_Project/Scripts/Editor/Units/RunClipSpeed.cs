using System;
using UnityEditor;
using UnityEngine;

namespace Stilargo.Editor.Units
{
    /// <summary>
    /// Measures how fast the run clip carries a soldier over the ground: how fast the body moves ahead of the foot
    /// that is planted on the ground. Units match their run playback to it, so their feet do not slide.
    /// </summary>
    public static class RunClipSpeed
    {
        private const int SampleCount = 120;
        private const float PlausibleMinimumMetresPerSecond = 1f;
        private const float PlausibleMaximumMetresPerSecond = 8f;

        public static float MeasureMetresPerSecond(GameObject avatarPrefab, AnimationClip clip)
        {
            if (avatarPrefab == null)
            {
                throw new ArgumentNullException(nameof(avatarPrefab));
            }

            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }

            GameObject avatar = UnityEngine.Object.Instantiate(avatarPrefab);
            bool wasAnimating = AnimationMode.InAnimationMode();
            if (!wasAnimating)
            {
                AnimationMode.StartAnimationMode();
            }

            try
            {
                Animator animator = avatar.GetComponentInChildren<Animator>();
                if (animator == null || !animator.isHuman)
                {
                    throw new InvalidOperationException($"{avatarPrefab.name} has no humanoid Animator to measure the run on.");
                }

                float speed = AveragePlantedFootSpeed(animator, clip);
                if (!(speed >= PlausibleMinimumMetresPerSecond && speed <= PlausibleMaximumMetresPerSecond))
                {
                    throw new InvalidOperationException($"Measured an implausible run speed of {speed:0.00} m/s from {clip.name} on {avatarPrefab.name}.");
                }

                return speed;
            }
            finally
            {
                if (!wasAnimating)
                {
                    AnimationMode.StopAnimationMode();
                }

                UnityEngine.Object.DestroyImmediate(avatar);
            }
        }

        // How fast the hips move ahead of the lowest (planted) foot. That is the ground speed whether the clip moves
        // its body forward or slides its feet back, so it does not depend on how root motion is imported.
        private static float AveragePlantedFootSpeed(Animator animator, AnimationClip clip)
        {
            Transform left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            float step = clip.length / SampleCount;
            float travelled = 0f;
            float plantedTime = 0f;
            float previousLead = 0f;
            bool wasLeftPlanted = false;
            for (int sample = 0; sample <= SampleCount; sample++)
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(animator.gameObject, clip, sample * step);
                AnimationMode.EndSampling();
                bool isLeftPlanted = left.position.y <= right.position.y;
                Transform planted = isLeftPlanted ? left : right;
                float lead = hips.position.z - planted.position.z;
                if (sample > 0 && isLeftPlanted == wasLeftPlanted && lead > previousLead)
                {
                    travelled += lead - previousLead;
                    plantedTime += step;
                }

                previousLead = lead;
                wasLeftPlanted = isLeftPlanted;
            }

            Debug.Log($"{clip.name}: {travelled:0.000} m over {plantedTime:0.000} s of a {clip.length:0.000} s cycle.");
            return plantedTime > 0f ? travelled / plantedTime : 0f;
        }
    }
}
