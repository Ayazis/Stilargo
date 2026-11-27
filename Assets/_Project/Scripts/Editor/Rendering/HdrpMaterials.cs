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
        public const string DecalShaderName = "HDRP/Decal";

        private const string AffectNormalProperty = "_AffectNormal";
        private const string AffectSmoothnessProperty = "_AffectSmoothness";

        private const string LitColorProperty = "_BaseColor";
        private const string UnlitColorProperty = "_UnlitColor";
        private const string SmoothnessProperty = "_Smoothness";
        private const string BaseColorMapProperty = "_BaseColorMap";
        private const string NormalMapProperty = "_NormalMap";
        private const string MaskMapProperty = "_MaskMap";
        private const string UvBaseProperty = "_UVBase";
        private const string TexWorldScaleProperty = "_TexWorldScale";
        private const float UvBaseUv0 = 0f;
        private const float UvBaseTriplanar = 5f;

        // Placeholder surfaces read as painted concrete/plastic rather than mirror-smooth.
        private const float PlaceholderSmoothness = 0.35f;

        public static Material CreateLit(Color color)
        {
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

        /// <summary>
        /// A decal that only tints albedo, with opacity from its base color map's alpha. HDRP only samples the map
        /// when one is assigned at validation, so pass a placeholder even if the real map is set at runtime.
        /// </summary>
        public static Material CreateAlbedoDecal(Texture colorMap)
        {
            if (colorMap == null)
            {
                throw new ArgumentNullException(nameof(colorMap));
            }

            var material = new Material(RequireShader(DecalShaderName));
            material.SetTexture(BaseColorMapProperty, colorMap);
            material.SetFloat(AffectNormalProperty, 0f);
            material.SetFloat(AffectSmoothnessProperty, 0f);
            HDMaterial.ValidateMaterial(material);
            return material;
        }

        public static void SetLitColor(Material material, Color color)
        {
            material.SetColor(LitColorProperty, color);
        }

        public static void SetSmoothness(Material material, float smoothness)
        {
            material.SetFloat(SmoothnessProperty, smoothness);
            HDMaterial.ValidateMaterial(material);
        }

        /// <summary>Projects the textures in world space, for scaled boxes whose own UVs would stretch.</summary>
        public static void ApplyTriplanarTextures(Material material, SurfaceTextures textures, float metresPerTile)
        {
            ApplyTextures(material, textures, metresPerTile);
            material.SetFloat(UvBaseProperty, UvBaseTriplanar);
            material.SetFloat(TexWorldScaleProperty, 1f / metresPerTile);
            material.SetTextureScale(BaseColorMapProperty, Vector2.one);
            HDMaterial.ValidateMaterial(material);
        }

        /// <summary>Maps the textures through the mesh's UVs, which must be in metres.</summary>
        public static void ApplyMetricUvTextures(Material material, SurfaceTextures textures, float metresPerTile)
        {
            ApplyTextures(material, textures, metresPerTile);
            material.SetFloat(UvBaseProperty, UvBaseUv0);
            material.SetTextureScale(BaseColorMapProperty, Vector2.one / metresPerTile);
            HDMaterial.ValidateMaterial(material);
        }

        /// <summary>Maps the textures through the mesh's own UVs, once, as authored characters expect.</summary>
        public static void ApplyUvTextures(Material material, SurfaceTextures textures)
        {
            ApplyTextures(material, textures, 1f);
            material.SetFloat(UvBaseProperty, UvBaseUv0);
            HDMaterial.ValidateMaterial(material);
        }

        private static void ApplyTextures(Material material, SurfaceTextures textures, float metresPerTile)
        {
            if (metresPerTile <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(metresPerTile), metresPerTile, "Tile size must be positive.");
            }

            material.SetTexture(BaseColorMapProperty, textures.Albedo);
            material.SetTexture(NormalMapProperty, textures.Normal);
            material.SetTexture(MaskMapProperty, textures.Mask);
        }

        private static void ApplyLit(Material material, Color color)
        {
            SetLitColor(material, color);
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
