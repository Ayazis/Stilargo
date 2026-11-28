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
            Mapping = SurfaceMapping.Projected;
            ParallaxCentimetres = 0f;
            IsWorn = false;
        }

        private HouseMaterialLook(HouseMaterialLook basis, SurfaceMapping mapping, float parallaxCentimetres)
            : this(basis.Color, basis.Smoothness, basis.TextureSet, basis.MetresPerTile)
        {
            Mapping = mapping;
            ParallaxCentimetres = parallaxCentimetres;
            IsWorn = basis.IsWorn;
        }

        private HouseMaterialLook(HouseMaterialLook basis, bool isWorn)
            : this(basis, basis.Mapping, basis.ParallaxCentimetres)
        {
            IsWorn = isWorn;
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

        /// <summary>
        /// A scanned set mapped through the mesh's UVs (in metres) with parallax depth, for surfaces whose pattern must
        /// follow the face, like roof tiles running down every slope.
        /// </summary>
        public static HouseMaterialLook ScannedRelief(Color tint, string textureSet, float metresPerTile, float parallaxCentimetres)
        {
            return new HouseMaterialLook(Scanned(tint, textureSet, metresPerTile), SurfaceMapping.MetricUvRelief, parallaxCentimetres);
        }

        /// <summary>A photo-scanned plant: its own texture set on its own UVs, lit from both sides.</summary>
        public static HouseMaterialLook ScannedFoliage(string textureSet)
        {
            return new HouseMaterialLook(Scanned(Color.white, textureSet, GenericMetresPerTile), SurfaceMapping.ScannedFoliage, 0f);
        }

        public Color Color { get; }

        public float Smoothness { get; }

        /// <summary>The <see cref="Rendering.SurfaceTextures"/> set projected over the colour, or null for none.</summary>
        public string TextureSet { get; }

        public float MetresPerTile { get; }

        public SurfaceMapping Mapping { get; }

        /// <summary>True when large-scale dirt and run-off are laid over the texture (see <c>Tools/Art/make_wear_detail_map.py</c>).</summary>
        public bool IsWorn { get; }

        /// <summary>Parallax depth for <see cref="SurfaceMapping.MetricUvRelief"/>; 0 otherwise.</summary>
        public float ParallaxCentimetres { get; }

        public bool IsTextured => TextureSet != null;

        /// <summary>The same look with weathering over it, for old brick, stone and wood.</summary>
        public HouseMaterialLook Worn()
        {
            if (!IsTextured)
            {
                throw new System.InvalidOperationException("Only textured looks can be worn: the wear is a detail map over the texture.");
            }

            return new HouseMaterialLook(this, true);
        }
    }
}
