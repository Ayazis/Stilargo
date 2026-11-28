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
    /// physically based sky with sparse volumetric clouds, SSGI, automatic exposure, horizon depth of field
    /// a thin aerial-perspective haze and neutral filmic grading.
    /// Safe to rerun. Batch: -executeMethod Stilargo.Editor.Rendering.PhotorealRenderingSetup.Apply
    /// </summary>
    public static class PhotorealRenderingSetup
    {
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string PipelinePath = SettingsFolder + "/HdrpPipeline.asset";
        private const int FullResolutionMipLimit = 0;

        private const float BloomIntensity = 0.15f;
        private const float VignetteIntensity = 0.2f;

        // Shadows: the RTS camera sees far, so cascades reach the whole city at a sharp resolution.
        private const int DirectionalShadowResolutionLow = 1024;
        private const int DirectionalShadowResolutionHigh = 4096;
        private const float MaxShadowDistanceMeters = 600f;
        private const int ShadowCascadeCount = 4;

        // Ambient occlusion: grounds wall bases and corners without crushing the sky-lit shadows to brown.
        private const float AmbientOcclusionIntensity = 0.8f;
        private const float AmbientOcclusionRadiusMeters = 1.5f;
        private const float AmbientOcclusionDirectLightingStrength = 0.15f;

        // Grading: ACES already adds contrast and the sun is warm, so the grade stays neutral and only
        // cools a touch so shadows read as sky-blue instead of orange.
        private const float ContrastPercent = 0f;
        private const float SaturationPercent = -3f;
        private const float WhiteBalanceTemperature = -4f;

        // Auto exposure meters the bright roofs; a small lift keeps the streets and shadow sides readable.
        private const float ExposureCompensationEv = 0.4f;

        // Aerial perspective: a thin, low haze. A long mean free path keeps the play area clear while
        // distant blocks fade slightly into the sky colour, which gives the city depth and scale.
        private const float FogMeanFreePathMeters = 1400f;
        private const float FogBaseHeightMeters = 0f;
        private const float FogMaximumHeightMeters = 120f;
        private const float FilmGrainIntensity = 0.12f;

        // Only the far horizon softens, so zooming never blurs the play area.
        private const float FarBlurStartMeters = 300f;
        private const float FarBlurEndMeters = 900f;

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
            settings.supportVolumetricClouds = true;
            ApplyShadowQuality(ref settings);
            pipeline.currentPlatformRenderPipelineSettings = settings;
            EditorUtility.SetDirty(pipeline);
            return pipeline;
        }

        private static void ApplyShadowQuality(ref RenderPipelineSettings settings)
        {
            settings.hdShadowInitParams.directionalShadowFilteringQuality = HDShadowFilteringQuality.High;
            settings.hdShadowInitParams.maxDirectionalShadowMapResolution = DirectionalShadowResolutionHigh;
            settings.hdShadowInitParams.shadowResolutionDirectional = new IntScalableSetting(
                new[] { DirectionalShadowResolutionLow, DirectionalShadowResolutionHigh, DirectionalShadowResolutionHigh, DirectionalShadowResolutionHigh },
                ScalableSettingSchemaId.With4Levels);
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
            ApplyShadows(profile);
            ApplyScreenSpaceEffects(profile);
            ApplyPostProcessing(profile);
            ApplyGrading(profile);
            EditorUtility.SetDirty(profile);
        }

        private static void ApplySky(VolumeProfile profile)
        {
            VisualEnvironment environment = GetOrAdd<VisualEnvironment>(profile);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            GetOrAdd<PhysicallyBasedSky>(profile);

            VolumetricClouds clouds = GetOrAdd<VolumetricClouds>(profile);
            clouds.enable.Override(true);
            clouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
        }

        private static void ApplyShadows(VolumeProfile profile)
        {
            HDShadowSettings shadows = GetOrAdd<HDShadowSettings>(profile);
            shadows.maxShadowDistance.Override(MaxShadowDistanceMeters);
            shadows.cascadeShadowSplitCount.Override(ShadowCascadeCount);
            GetOrAdd<MicroShadowing>(profile).enable.Override(true);
        }

        private static void ApplyExposureAndFog(VolumeProfile profile)
        {
            Exposure exposure = GetOrAdd<Exposure>(profile);
            exposure.mode.Override(ExposureMode.AutomaticHistogram);
            exposure.compensation.Override(ExposureCompensationEv);

            // Earlier dense fog greyed out the whole city; this one only tints the distance.
            Fog fog = GetOrAdd<Fog>(profile);
            fog.enabled.Override(true);
            fog.colorMode.Override(FogColorMode.SkyColor);
            fog.meanFreePath.Override(FogMeanFreePathMeters);
            fog.baseHeight.Override(FogBaseHeightMeters);
            fog.maximumHeight.Override(FogMaximumHeightMeters);
            fog.enableVolumetricFog.Override(false);
        }

        private static void ApplyScreenSpaceEffects(VolumeProfile profile)
        {
            ScreenSpaceAmbientOcclusion ambientOcclusion = GetOrAdd<ScreenSpaceAmbientOcclusion>(profile);
            ambientOcclusion.intensity.Override(AmbientOcclusionIntensity);
            ambientOcclusion.radius.Override(AmbientOcclusionRadiusMeters);
            ambientOcclusion.directLightingStrength.Override(AmbientOcclusionDirectLightingStrength);
            ambientOcclusion.quality.Override((int)ScalableSettingLevelParameter.Level.High);
            GlobalIllumination globalIllumination = GetOrAdd<GlobalIllumination>(profile);
            // SSGI off: at RTS distances it flickered and crushed the alleys to black.
            globalIllumination.enable.Override(false);
            globalIllumination.quality.Override((int)ScalableSettingLevelParameter.Level.Medium);
            GetOrAdd<ScreenSpaceReflection>(profile).enabled.Override(true);
            GetOrAdd<ContactShadows>(profile).enable.Override(true);
        }

        private static void ApplyPostProcessing(VolumeProfile profile)
        {
            GetOrAdd<Tonemapping>(profile).mode.Override(TonemappingMode.ACES);
            GetOrAdd<Bloom>(profile).intensity.Override(BloomIntensity);
            GetOrAdd<Vignette>(profile).intensity.Override(VignetteIntensity);

            DepthOfField depthOfField = GetOrAdd<DepthOfField>(profile);
            depthOfField.focusMode.Override(DepthOfFieldMode.Manual);
            depthOfField.farFocusStart.Override(FarBlurStartMeters);
            depthOfField.farFocusEnd.Override(FarBlurEndMeters);
        }

        private static void ApplyGrading(VolumeProfile profile)
        {
            ColorAdjustments colorAdjustments = GetOrAdd<ColorAdjustments>(profile);
            colorAdjustments.contrast.Override(ContrastPercent);
            colorAdjustments.saturation.Override(SaturationPercent);
            GetOrAdd<WhiteBalance>(profile).temperature.Override(WhiteBalanceTemperature);

            FilmGrain filmGrain = GetOrAdd<FilmGrain>(profile);
            filmGrain.type.Override(FilmGrainLookup.Thin1);
            filmGrain.intensity.Override(FilmGrainIntensity);
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
