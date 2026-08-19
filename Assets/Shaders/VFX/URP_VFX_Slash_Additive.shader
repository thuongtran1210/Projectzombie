Shader "ProjectZombie/VFX/Slash_Additive"
{
    Properties
    {
        [Header(Textures and UV)]
        _MainTex ("Main Slash Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture (Dissolve/Scroll)", 2D) = "white" {}
        _Speed ("UV Scroll Speed (X, Y)", Vector) = (1.0, 0.0, 0, 0)
        [Toggle(_USE_POLAR_COORDS)] _UsePolar ("Use Polar Coordinates (Circular Arc)", Float) = 0.0

        [Header(Colors and Glow)]
        [HDR] _CoreColor ("Core Bright Color (Lõi Sáng)", Color) = (2.0, 2.0, 2.0, 1.0)
        [HDR] _EdgeColor ("Edge Element Color (Viền Nguyên Tố)", Color) = (1.0, 0.4, 0.1, 1.0)
        _CoreThreshold ("Core Intensity Threshold", Range(0.1, 1.0)) = 0.7

        [Header(Dissolve Settings)]
        _DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.0
        _DissolveSoftness ("Dissolve Softness", Range(0.01, 0.5)) = 0.1

        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1 // One (Additive)
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "RenderPipeline"="UniversalPipeline" 
        }

        Blend [_SrcBlend] [_DstBlend]
        ZWrite Off
        Cull Off

        Pass
        {
            Name "VFX_Slash_Additive"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma shader_feature_local _USE_POLAR_COORDS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
                float2 noiseUV    : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float4 _Speed;
                float4 _CoreColor;
                float4 _EdgeColor;
                float _CoreThreshold;
                float _DissolveAmount;
                float _DissolveSoftness;
            CBUFFER_END

            // Chuyển đổi tọa độ cực (Polar Coordinates) cho vệt kiếm hình cung tròn
            float2 ToPolarCoordinates(float2 uv)
            {
                float2 delta = uv - float2(0.5, 0.5);
                float radius = length(delta) * 2.0;
                float angle = atan2(delta.y, delta.x) / 6.28318530718 + 0.5;
                return float2(angle, radius);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;

                float2 baseUV = input.uv;
                #if defined(_USE_POLAR_COORDS)
                    baseUV = ToPolarCoordinates(input.uv);
                #endif

                output.uv = TRANSFORM_TEX(baseUV, _MainTex);
                output.noiseUV = TRANSFORM_TEX(baseUV, _NoiseTex) + _Speed.xy * _Time.y;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. Đọc mẫu Texture chính & Noise
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.noiseUV).r;

                // 2. Tính toán Dissolve tan biến theo mép
                half dissolveCutoff = _DissolveAmount * (1.0 + _DissolveSoftness);
                half dissolveMask = smoothstep(dissolveCutoff - _DissolveSoftness, dissolveCutoff, noise);

                half finalAlpha = mainTex.a * dissolveMask * input.color.a;

                // 3. Tách biệt Lõi sáng (Core) và Viền màu Ngũ Hành (Edge)
                half luminance = dot(mainTex.rgb, half3(0.299, 0.587, 0.114));
                half coreFactor = smoothstep(_CoreThreshold, 1.0, luminance);

                half3 mixedColor = lerp(_EdgeColor.rgb * input.color.rgb, _CoreColor.rgb, coreFactor);

                return half4(mixedColor * finalAlpha, finalAlpha);
            }
            ENDHLSL
        }
    }
}
