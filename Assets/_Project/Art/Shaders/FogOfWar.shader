// Full-screen fog of war. Runs after post processing, so auto exposure can't brighten the fog back up, and it covers
// every surface (ground, roofs, water, units) because it works from the depth buffer instead of projecting onto materials.
Shader "Stilargo/FogOfWar"
{
    Properties
    {
        _FogMap("Fog Map", 2D) = "black" {}
        _FogMapWorldSize("Fog Map World Size (m)", Vector) = (1, 1, 0, 0)
        _OutsideColor("Outside Map Color", Color) = (0.02, 0.02, 0.025, 1)
    }

    HLSLINCLUDE

    #pragma vertex Vert
    #pragma target 4.5
    #pragma only_renderers d3d11 vulkan metal

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D(_FogMap);
    SAMPLER(sampler_FogMap);
    float4 _FogMapWorldSize;
    float4 _OutsideColor;

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);

        // Leave the sky alone: there is nothing on it to hide.
        if (depth == UNITY_RAW_FAR_CLIP_VALUE)
        {
            return float4(0, 0, 0, 0);
        }

        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 positionWS = GetAbsolutePositionWS(posInput.positionWS);
        float2 uv = positionWS.xz / _FogMapWorldSize.xy;
        if (any(uv < 0) || any(uv > 1))
        {
            return _OutsideColor;
        }

        return SAMPLE_TEXTURE2D_LOD(_FogMap, sampler_FogMap, uv, 0);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "Custom Pass 0"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
