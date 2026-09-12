Shader "FlappyTerminator/WarpShockWave"
{
    Properties
    {
        _Strength ("Distortion Strength", Range(0, 0.2)) = 0.05
        _RingRadius ("Ring Radius", Range(0, 0.5)) = 0.38
        _RingWidth ("Ring Width", Range(0.01, 0.5)) = 0.12
        _Glow ("Glow", Range(0, 1)) = 0.15
        _GlowColor ("Glow Color", Color) = (0.6, 0.85, 1, 1)
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
        Blend Off

        Pass
        {
            Name "WarpShockWave"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _RingRadius;
                float _RingWidth;
                float _Glow;
                float4 _GlowColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.screenPosition = ComputeScreenPos(output.positionCS);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centered = input.uv - 0.5;
                float distance = length(centered);

                float ring = 1.0 - smoothstep(0.0, _RingWidth, abs(distance - _RingRadius));

                if (ring < 0.01)
                    discard;

                float2 direction = distance > 0.0001 ? centered / distance : float2(0.0, 0.0);
                float2 screenUV = input.screenPosition.xy / input.screenPosition.w;

                screenUV += direction * ring * _Strength;

                half3 scene = SAMPLE_TEXTURE2D_X(
                    _CameraSortingLayerTexture,
                    sampler_CameraSortingLayerTexture,
                    screenUV).rgb;

                return half4(scene + _GlowColor.rgb * ring * _Glow * _Strength * 20.0, 1.0);
            }
            ENDHLSL
        }
    }
}
