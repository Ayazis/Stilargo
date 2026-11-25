using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Makes HDRP the project's pipeline and applies the settings our photoreal target needs: linear color,
    /// physical light units, high-quality shadows, screen-space effects, and a global volume with a
    /// physically based sky, automatic exposure, fog and tonemapping.
    /// Safe to rerun. Batch: -executeMethod Stilargo.Editor.Rendering.PhotorealRenderingSetup.Apply
    /// </summary>
    public static class PhotorealRenderingSetup
    {
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string PipelinePath = SettingsFolder + "/HdrpPipeline.asset";
        private const int FullResolutionMipLimit = 0;

        private const float FogMeanFreePathMetres = 700f;
        private const float BloomIntensity = 0.15f;
        private const float VignetteIntensity = 0.2f;

        [MenuItem("Stilargo/Apply Photoreal Rendering Settings")]
        public static void Apply()
        {
            ApplyPlayerAndGraphicsSettings();
            ApplyQualitySettings();
            GraphicsSettings.defaultRenderPipeline = LoadOrCreatePipeline();
            LoadOrCreateVolumeProfile();
            AssetDatabase.SaveAssets();
            Debug.Log("Photoreal HDRP settings applied.");
        }

        private static void ApplyPlayerAndGraphicsSettings()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = true;
        }

        private static void ApplyQualitySettings()
        {
            int originalLevel = QualitySettings.GetQualityLevel();
            for (int level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                // Levels fall back to the default pipeline; a leftover URP override would win otherwise.
                QualitySettings.renderPipeline = null;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.globalTextureMipmapLimit = FullResolutionMipLimit;
            }

            QualitySettings.SetQualityLevel(originalLevel, false);
        }

        private static HDRenderPipelineAsset LoadOrCreatePipeline()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            RenderPipelineSettings settings = pipeline.currentPlatformRenderPipelineSettings;
            settings.supportSSAO = true;
            settings.supportSSR = true;
            settings.supportSSGI = true;
            settings.supportVolumetrics = true;
            settings.supportMotionVectors = true;
            pipeline.currentPlatformRenderPipelineSettings = settings;
            EditorUtility.SetDirty(pipeline);
            return pipeline;
        }

        private static void LoadOrCreateVolumeProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PhotorealSceneDefaults.VolumeProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PhotorealSceneDefaults.VolumeProfilePath);
            }

            ApplySky(profile);
            ApplyExposureAndFog(profile);
            ApplyScreenSpaceEffects(profile);
            ApplyPostProcessing(profile);
            EditorUtility.SetDirty(profile);
        }

        private static void ApplySky(VolumeProfile profile)
        {
            VisualEnvironment environment = GetOrAdd<VisualEnvironment>(profile);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            GetOrAdd<PhysicallyBasedSky>(profile);
        }

        private static void ApplyExposureAndFog(VolumeProfile profile)
        {
            GetOrAdd<Exposure>(profile).mode.Override(ExposureMode.AutomaticHistogram);

            Fog fog = GetOrAdd<Fog>(profile);
            fog.enabled.Override(true);
            fog.meanFreePath.Override(FogMeanFreePathMetres);
            fog.enableVolumetricFog.Override(true);
        }

        private static void ApplyScreenSpaceEffects(VolumeProfile profile)
        {
            GetOrAdd<ScreenSpaceAmbientOcclusion>(profile).intensity.Override(1f);
            GetOrAdd<ScreenSpaceReflection>(profile).enabled.Override(true);
            GetOrAdd<ContactShadows>(profile).enable.Override(true);
        }

        private static void ApplyPostProcessing(VolumeProfile profile)
        {
            GetOrAdd<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            GetOrAdd<Bloom>(profile).intensity.Override(BloomIntensity);
            GetOrAdd<Vignette>(profile).intensity.Override(VignetteIntensity);
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (profile.TryGet(out T existing))
            {
                return existing;
            }

            T component = profile.Add<T>();
            component.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

    }
}
