Shader "Custom/ShockwaveDistortionURP"
{
    Properties
    {
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.1
        _RippleSpeed ("Ripple Speed", Range(0, 10)) = 1
        _RippleFrequency ("Ripple Frequency", Range(0, 50)) = 10
        _RippleWidth ("Ripple Width", Range(0.1, 2)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "Shockwave"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _DistortionStrength;
                float _RippleSpeed;
                float _RippleFrequency;
                float _RippleWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
    float dist = distance(IN.uv, center);
    
    if (dist > 0.5)
        discard;
    
    float time = _Time.y * _RippleSpeed;
    float wave = sin(dist * _RippleFrequency - time);
    
    float ringDist = abs(dist - frac(time / 6.28));
    float falloff = 1.0 - smoothstep(0, _RippleWidth, ringDist);
    
    if (falloff < 0.01)
        discard;
    
    float2 direction = normalize(IN.uv - center);
    float2 distortion = direction * wave * _DistortionStrength * falloff;
    
    float2 screenUV = IN.screenPos.xy / IN.screenPos.w + distortion;
    half3 color = SampleSceneColor(screenUV);
    
    // Добавляем белое свечение для видимости волны
    color += falloff * 0.3;
    
    return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}