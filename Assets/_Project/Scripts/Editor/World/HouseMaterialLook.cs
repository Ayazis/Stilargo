using UnityEngine;

namespace Stilargo.Editor.World
{
    /// <summary>How one material slot of a Blender-built house should look as an HDRP material.</summary>
    public readonly struct HouseMaterialLook
    {
        private const string PlasterTextures = "white_plaster_rough_01";
        private const string WeatheringTextures = "weathered_grime";
        private const float GenericMetresPerTile = 2.5f;

        // Scanned sets carry smoothness in their mask map, which HDRP reads instead of this value.
        private const float ScannedSmoothness = 1f;

        private HouseMaterialLook(Color color, float smoothness, string textureSet, float metresPerTile)
        {
            Color = color;
            Smoothness = smoothness;
            TextureSet = textureSet;
            MetresPerTile = metresPerTile;
        }

        /// <summary>Plain colour, for paint, metal, glass and fired clay.</summary>
        public static HouseMaterialLook Painted(Color color, float smoothness)
        {
            return new HouseMaterialLook(color, smoothness, null, GenericMetresPerTile);
        }

        /// <summary>Colour over the plaster texture, for walls and stone.</summary>
        public static HouseMaterialLook Plastered(Color color, float smoothness)
        {
            return new HouseMaterialLook(color, smoothness, PlasterTextures, GenericMetresPerTile);
        }

        /// <summary>Colour under blotchy dirt and run-off (<c>Tools/Art/make_weathering_texture.py</c>), for old surfaces.</summary>
        public static HouseMaterialLook Weathered(Color color, float smoothness)
        {
            return new HouseMaterialLook(color, smoothness, WeatheringTextures, GenericMetresPerTile);
        }

        /// <summary>A scanned texture set at its real-world size, tinted by a near-white colour.</summary>
        public static HouseMaterialLook Scanned(Color tint, string textureSet, float metresPerTile)
        {
            return new HouseMaterialLook(tint, ScannedSmoothness, textureSet, metresPerTile);
        }

        public Color Color { get; }

        public float Smoothness { get; }

        /// <summary>The <see cref="Rendering.SurfaceTextures"/> set projected over the colour, or null for none.</summary>
        public string TextureSet { get; }

        public float MetresPerTile { get; }

        public bool IsTextured => TextureSet != null;
    }
}
