using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Shared per-scene rendering defaults for the scene builders, so every generated scene gets the same
    /// TAA camera, physical sun and global sky/exposure/post volume.
    /// </summary>
    public static class PhotorealSceneDefaults
    {
        public const string VolumeProfilePath = "Assets/_Project/Settings/PhotorealVolumeProfile.asset";

        // Midday daylight: about 6000 K and roughly 100,000 lux of direct sun.
        private const float SunTemperatureKelvin = 6000f;
        private const float SunIlluminanceLux = 100000f;
        private const string VolumeName = "Global Volume";

        public static void ConfigureCamera(Camera camera)
        {
            // HDRP may already have added its camera data when the Camera component was created.
            HDAdditionalCameraData data = camera.GetComponent<HDAdditionalCameraData>();
            if (data == null)
            {
                data = camera.gameObject.AddComponent<HDAdditionalCameraData>();
            }

            data.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
            data.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.High;
        }

        public static Light CreateSun(string name)
        {
            var sunObject = new GameObject(name);
            sunObject.AddHDLight(LightType.Directional);
            Light sun = sunObject.GetComponent<Light>();
            sun.useColorTemperature = true;
            sun.colorTemperature = SunTemperatureKelvin;
            sun.color = Color.white;
            sun.lightUnit = LightUnit.Lux;
            sun.intensity = SunIlluminanceLux;
            sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun;
            return sun;
        }

        public static void CreateGlobalVolume()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException($"No volume profile at {VolumeProfilePath}. Run Stilargo → Apply Photoreal Rendering Settings first.");
            }

            var volumeObject = new GameObject(VolumeName);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }
    }
}
