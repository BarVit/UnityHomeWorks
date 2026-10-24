Shader "FlappyTerminator/HexArmor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (0.55, 1, 0.55, 1)
        _FillColor ("Fill Color", Color) = (0.1, 0.75, 0.25, 0.3)
        _PulseColor ("Pulse Color", Color) = (0.2, 1, 0.75, 1)
        _HexSize ("Hex Size (sprite units)", Float) = 0.04
        _EdgeWidth ("Edge Width (share of hex)", Range(0.01, 0.4)) = 0.12
        _PulseSpeed ("Pulses per Second", Float) = 1.5
        _PulseMin ("Pulse Minimum", Range(0, 1)) = 0.45
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
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "HexArmor"

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
                float2 hexUV : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _EdgeColor;
                float4 _FillColor;
                float4 _PulseColor;
                float _HexSize;
                float _EdgeWidth;
                float _PulseSpeed;
                float _PulseMin;
            CBUFFER_END

            static const float2 HexStep = float2(1.0, 1.7320508);

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 position = float3(input.positionOS.xy * unity_SpriteProps.xy, input.positionOS.z);

                output.positionCS = TransformObjectToHClip(position);
                output.color = input.color * unity_SpriteColor;
                output.uv = input.uv;
                output.hexUV = position.xy / max(_HexSize, 0.0001);

                return output;
            }

            float2 Wrap(float2 value)
            {
                return value - HexStep * floor(value / HexStep) - HexStep * 0.5;
            }

            float EdgeDistance(float2 position)
            {
                float2 a = Wrap(position);
                float2 b = Wrap(position - HexStep * 0.5);
                float2 cell = dot(a, a) < dot(b, b) ? a : b;
                float2 folded = abs(cell);
                float radius = max(dot(folded, normalize(HexStep)), folded.x);

                return 0.5 - radius;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;

                if (alpha < 0.05)
                    discard;

                float edge = 1.0 - smoothstep(_EdgeWidth * 0.5, _EdgeWidth, EdgeDistance(input.hexUV));
                float wave = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 6.2831853);
                float pulse = lerp(_PulseMin, 1.0, wave);
                float3 edgeColor = lerp(_EdgeColor.rgb, _PulseColor.rgb, wave);
                float3 color = lerp(_FillColor.rgb, edgeColor, edge);
                float shade = lerp(_FillColor.a, _EdgeColor.a, edge) * pulse;

                return half4(color, alpha * shade * input.color.a);
            }
            ENDHLSL
        }
    }
}
