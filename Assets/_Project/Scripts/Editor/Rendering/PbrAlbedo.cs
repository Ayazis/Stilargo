using System;
using UnityEngine;

namespace Stilargo.Editor.Rendering
{
    /// <summary>
    /// Checks that a base colour could belong to a real non-metallic surface. Real materials never reflect all
    /// or none of the light, and colours outside that range look wrong under physically based lighting.
    /// Bounds are in sRGB, like the colour picker: charcoal is about 30/255 and fresh snow about 240/255.
    /// </summary>
    public static class PbrAlbedo
    {
        public const float DarkestChannel = 30f / 255f;
        public const float BrightestChannel = 240f / 255f;

        public static bool IsPlausible(Color srgb)
        {
            float brightest = Mathf.Max(srgb.r, Mathf.Max(srgb.g, srgb.b));
            return brightest >= DarkestChannel && brightest <= BrightestChannel;
        }

        public static void Require(Color srgb)
        {
            if (!IsPlausible(srgb))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(srgb),
                    srgb,
                    $"Albedo needs its brightest sRGB channel between {DarkestChannel:0.###} and {BrightestChannel:0.###}.");
            }
        }
    }
}
