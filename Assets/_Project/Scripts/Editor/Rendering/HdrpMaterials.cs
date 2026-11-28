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
        public const string FogOfWarShaderName = "Stilargo/FogOfWar";

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
        private const string HeightMapProperty = "_HeightMap";
        private const string HeightParametrizationProperty = "_HeightMapParametrization";
        private const string ParallaxAmplitudeProperty = "_HeightPoMAmplitude";
        private const string DisplacementModeProperty = "_DisplacementMode";
        private const string ParallaxMaxSamplesProperty = "_PPDMaxSamples";
        private const float HeightAsAmplitude = 1f;
        private const float DisplacementPixel = 2f;
        private const float ParallaxMaxSamples = 32f;
        private const string DoubleSidedProperty = "_DoubleSidedEnable";
        private const string DoubleSidedNormalModeProperty = "_DoubleSidedNormalMode";
        private const float DoubleSidedNormalFlip = 0f;
        private const string DetailMapProperty = "_DetailMap";
        private const string DetailAlbedoScaleProperty = "_DetailAlbedoScale";
        private const string DetailNormalScaleProperty = "_DetailNormalScale";
        private const string DetailSmoothnessScaleProperty = "_DetailSmoothnessScale";

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

        /// <summary>The full-screen fog of war material; <c>FogOverlay</c> assigns its fog map at runtime.</summary>
        public static Material CreateFogOfWarScreen()
        {
            return new Material(RequireShader(FogOfWarShaderName));
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

        /// <summary>
        /// Adds parallax occlusion mapping from a height map, so deep relief such as roof tiles has real depth.
        /// It needs UV mapping, not triplanar.
        /// </summary>
        public static void ApplyParallax(Material material, Texture2D height, float depthCentimetres)
        {
            if (height == null)
            {
                throw new ArgumentNullException(nameof(height));
            }

            material.SetTexture(HeightMapProperty, height);
            material.SetFloat(HeightParametrizationProperty, HeightAsAmplitude);
            material.SetFloat(ParallaxAmplitudeProperty, depthCentimetres);
            material.SetFloat(DisplacementModeProperty, DisplacementPixel);
            material.SetFloat(ParallaxMaxSamplesProperty, ParallaxMaxSamples);
            HDMaterial.ValidateMaterial(material);
        }

        /// <summary>
        /// Lays a detail map (such as large-scale grime) over the base textures, through the same mapping; tiling is
        /// relative to the base texture's tile. Null removes it.
        /// </summary>
        public static void SetDetail(Material material, Texture2D detail, float tilingRelativeToBase)
        {
            if (tilingRelativeToBase <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(tilingRelativeToBase), tilingRelativeToBase, "Tiling must be positive.");
            }

            material.SetTexture(DetailMapProperty, detail);
            material.SetTextureScale(DetailMapProperty, Vector2.one * tilingRelativeToBase);
            material.SetFloat(DetailAlbedoScaleProperty, 1f);
            material.SetFloat(DetailNormalScaleProperty, 0f);
            material.SetFloat(DetailSmoothnessScaleProperty, 1f);
            HDMaterial.ValidateMaterial(material);
        }

        /// <summary>Renders both faces, with the normal flipped on the back, for single-sheet geometry such as leaves.</summary>
        public static void MakeDoubleSided(Material material)
        {
            material.SetFloat(DoubleSidedProperty, 1f);
            material.SetFloat(DoubleSidedNormalModeProperty, DoubleSidedNormalFlip);
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
