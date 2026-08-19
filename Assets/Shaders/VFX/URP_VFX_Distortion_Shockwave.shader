Shader "ProjectZombie/VFX/Distortion_Shockwave"
{
    Properties
    {
        [Header(Shockwave Mask and Normal)]
        _MainTex ("Shockwave Ring Mask (R=Mask, G=Distort)", 2D) = "white" {}
        _NormalMap ("Normal / Flow Map (Optional)", 2D) = "bump" {}

        [Header(Distortion Settings)]
        _DistortionStrength ("Distortion Strength (Độ Biến Dạng)", Range(0.0, 0.2)) = 0.05
        _WaveWidth ("Wave Ring Width", Range(0.01, 1.0)) = 0.2

        [Header(Glow and Tint)]
        [HDR] _TintColor ("Wave Ring Tint (Hào Quang Viền)", Color) = (1.0, 1.0, 1.0, 0.5)
        _Falloff ("Edge Soft Falloff", Range(0.1, 2.0)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent+100" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NormalMap_ST;
                float4 _TintColor;
                float _DistortionStrength;
                float _WaveWidth;
                float _Falloff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.color = input.color;
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUV = (input.uv - 0.5) * 2.0;
                float dist = length(centeredUV);

                float ring = 1.0 - smoothstep(1.0 - _WaveWidth, 1.0, dist);
                float ringInner = smoothstep(1.0 - _WaveWidth * 2.0, 1.0 - _WaveWidth, dist);
                float waveMask = saturate(ring * ringInner);

                float2 distortOffset = (centeredUV / max(dist, 0.001)) * _DistortionStrength * waveMask * input.color.a;

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortedScreenUV = screenUV + distortOffset;

                #if defined(_SURFACE_TYPE_TRANSPARENT) || defined(REQUIRES_OPAQUE_TEXTURE)
                    half3 sceneColor = SampleSceneColor(distortedScreenUV);
                #else
                    half3 sceneColor = half3(1.0, 1.0, 1.0);
                #endif

                half3 finalRGB = sceneColor + _TintColor.rgb * waveMask * _TintColor.a;
                half finalAlpha = saturate(waveMask * _TintColor.a * input.color.a);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 screenPos    : TEXCOORD0;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NormalMap_ST;
                float4 _TintColor;
                float _DistortionStrength;
                float _WaveWidth;
                float _Falloff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.color = input.color;
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 centeredUV = (input.uv - 0.5) * 2.0;
                float dist = length(centeredUV);

                float ring = 1.0 - smoothstep(1.0 - _WaveWidth, 1.0, dist);
                float ringInner = smoothstep(1.0 - _WaveWidth * 2.0, 1.0 - _WaveWidth, dist);
                float waveMask = saturate(ring * ringInner);

                float2 distortOffset = (centeredUV / max(dist, 0.001)) * _DistortionStrength * waveMask * input.color.a;

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 distortedScreenUV = screenUV + distortOffset;

                #if defined(_SURFACE_TYPE_TRANSPARENT) || defined(REQUIRES_OPAQUE_TEXTURE)
                    half3 sceneColor = SampleSceneColor(distortedScreenUV);
                #else
                    half3 sceneColor = half3(1.0, 1.0, 1.0);
                #endif

                half3 finalRGB = sceneColor + _TintColor.rgb * waveMask * _TintColor.a;
                half finalAlpha = saturate(waveMask * _TintColor.a * input.color.a);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
