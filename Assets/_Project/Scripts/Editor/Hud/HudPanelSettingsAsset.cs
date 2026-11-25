using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Stilargo.Editor.Hud
{
    /// <summary>
    /// Creates the HUD's panel settings asset with Unity's default runtime theme. It has to be an asset: a
    /// PanelSettings made at runtime has no theme, so buttons and text would not render.
    /// </summary>
    public static class HudPanelSettingsAsset
    {
        private const string Folder = "Assets/_Project/UI";
        private const string ThemePath = Folder + "/HudTheme.tss";
        private const string SettingsPath = Folder + "/HudPanelSettings.asset";
        private const string DefaultThemeImport = "@import url(\"unity-theme://default\");\n";
        private const int ReferenceWidthPixels = 1920;
        private const int ReferenceHeightPixels = 1080;
        private const float ScreenMatchHeightBias = 0.5f;

        public static PanelSettings LoadOrCreate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(SettingsPath);
            if (existing != null)
            {
                return existing;
            }

            PanelSettings settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.themeStyleSheet = LoadOrCreateTheme();
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(ReferenceWidthPixels, ReferenceHeightPixels);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = ScreenMatchHeightBias;
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static ThemeStyleSheet LoadOrCreateTheme()
        {
            if (!File.Exists(ThemePath))
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(ThemePath, DefaultThemeImport);
                AssetDatabase.ImportAsset(ThemePath);
            }

            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            if (theme == null)
            {
                throw new InvalidOperationException($"{ThemePath} did not import as a ThemeStyleSheet.");
            }

            return theme;
        }
    }
}
