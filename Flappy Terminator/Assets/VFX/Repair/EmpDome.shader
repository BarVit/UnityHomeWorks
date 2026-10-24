Shader "FlappyTerminator/EmpDome"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _FillColor ("Fill Color", Color) = (0.2, 0.5, 1, 0.16)
        _RimColor ("Rim Color", Color) = (0.55, 0.85, 1, 0.65)
        _RimWidth ("Rim Width", Range(0.01, 0.5)) = 0.08
        _RippleStrength ("Ripple Strength", Range(0, 0.2)) = 0.05
        _RippleCount ("Ripple Count", Float) = 14
        _RippleSpeed ("Ripple Speed", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "EmpDome"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FillColor;
                float4 _RimColor;
                float _RimWidth;
                float _RippleStrength;
                float _RippleCount;
                float _RippleSpeed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 position = float3(input.positionOS.xy * unity_SpriteProps.xy, input.positionOS.z);

                output.positionCS = TransformObjectToHClip(position);
                output.color = input.color * unity_SpriteColor;
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float distance = length(input.uv * 2.0 - 1.0);

                if (distance > 1.0)
                    discard;

                float rim = smoothstep(1.0 - _RimWidth, 1.0, distance);
                float edge = 1.0 - smoothstep(0.985, 1.0, distance);
                float ripple = _RippleStrength * sin(distance * _RippleCount - _Time.y * _RippleSpeed);
                float4 color = lerp(_FillColor, _RimColor, rim);

                return half4(color.rgb, saturate(color.a + ripple) * edge * input.color.a);
            }
            ENDHLSL
        }
    }
}
