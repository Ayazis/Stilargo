using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Creates materials for HDRP, so every builder makes them the same way.
    /// HDRP derives keywords and passes from its properties; ValidateMaterial applies them like the inspector does.
    /// </summary>
    public static class HdrpMaterials
    {
        public const string LitShaderName = "HDRP/Lit";
        public const string UnlitShaderName = "HDRP/Unlit";
        public const string UnlitColorMapProperty = "_UnlitColorMap";

        private const string LitColorProperty = "_BaseColor";
        private const string UnlitColorProperty = "_UnlitColor";

        // Placeholder surfaces read as painted concrete/plastic rather than mirror-smooth.
        private const float PlaceholderSmoothness = 0.35f;

        public static Material CreateLit(Color color)
        {
            PbrAlbedo.Require(color);
            var material = new Material(RequireShader(LitShaderName));
            ApplyLit(material, color);
            return material;
        }

        public static Material CreateUnlit(Color color)
        {
            var material = new Material(RequireShader(UnlitShaderName));
            ApplyUnlit(material, color);
            return material;
        }

        public static Material CreateTransparentUnlit()
        {
            Material material = CreateUnlit(Color.white);
            HDMaterial.SetSurfaceType(material, true);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        private static void ApplyLit(Material material, Color color)
        {
            material.SetColor(LitColorProperty, color);
            material.SetFloat("_Smoothness", PlaceholderSmoothness);
            HDMaterial.ValidateMaterial(material);
        }

        private static void ApplyUnlit(Material material, Color color)
        {
            material.SetColor(UnlitColorProperty, color);
            HDMaterial.ValidateMaterial(material);
        }

        private static Shader RequireShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Shader '{shaderName}' not found. Is HDRP installed?");
            }

            return shader;
        }
    }
}
