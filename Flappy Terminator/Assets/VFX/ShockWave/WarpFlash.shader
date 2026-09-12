Shader "FlappyTerminator/WarpFlash"
{
    Properties
    {
        _Strength ("Strength", Range(0, 4)) = 2
        _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _EdgeColor ("Edge Color", Color) = (0.35, 0.65, 1, 1)
        _CoreSize ("Core Size", Range(0.01, 0.5)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        ZWrite Off
        Cull Off
        Blend One One

        Pass
        {
            Name "WarpFlash"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float4 _CoreColor;
                float4 _EdgeColor;
                float _CoreSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distance = length(input.uv - 0.5);
                float falloff = saturate(1.0 - distance * 2.0);
                float glow = falloff * falloff;
                float core = 1.0 - smoothstep(0.0, _CoreSize, distance);

                half3 color = lerp(_EdgeColor.rgb, _CoreColor.rgb, core);

                return half4(color * glow * _Strength, 0.0);
            }
            ENDHLSL
        }
    }
}
