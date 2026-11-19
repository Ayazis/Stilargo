using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Creates the project's URP pipeline asset and makes it the default pipeline. The project was created
    /// without one, so it silently ran the Built-in pipeline and URP materials rendered magenta.
    /// Quality levels keep no override, so they all fall back to this default.
    /// </summary>
    public static class UrpPipelineSetup
    {
        private const string Folder = "Assets/_Project/Settings";
        private const string RendererPath = Folder + "/UrpRenderer.asset";
        private const string PipelinePath = Folder + "/UrpPipeline.asset";

        [MenuItem("Stilargo/Set Up URP Pipeline")]
        public static void SetUp()
        {
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                Debug.Log($"A default render pipeline is already assigned: {GraphicsSettings.defaultRenderPipeline.name}.");
                return;
            }

            EnsureFolder();
            UniversalRendererData rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, RendererPath);

            UniversalRenderPipelineAsset pipeline = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(pipeline, PipelinePath);

            GraphicsSettings.defaultRenderPipeline = pipeline;
            AssetDatabase.SaveAssets();
            Debug.Log($"URP pipeline created at {PipelinePath} and set as default.");
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Settings");
            }
        }
    }
}
