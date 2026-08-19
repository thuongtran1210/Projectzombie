Shader "ProjectZombie/VFX/GroundDecal_Dissolve"
{
    Properties
    {
        [Header(Decal and Pattern)]
        _MainTex ("Ground Decal Texture", 2D) = "white" {}
        _NoiseTex ("Dissolve Noise Texture", 2D) = "white" {}

        [Header(Colors and Burn Edge)]
        [HDR] _Color ("Decal Main Color (Màu Cơ Bản)", Color) = (1.0, 1.0, 1.0, 1.0)
        [HDR] _BurnColor ("Burn Edge Color (Viền Cháy Sáng)", Color) = (2.0, 0.8, 0.2, 1.0)
        _BurnWidth ("Burn Edge Width", Range(0.01, 0.3)) = 0.08

        [Header(Dissolve Progress)]
        _DissolveAmount ("Dissolve Progress (0=Hiện Rõ, 1=Biến Mất)", Range(0.0, 1.0)) = 0.0
        _NoiseTiling ("Noise Tiling", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent-50" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

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
                float4 _Color;
                float4 _BurnColor;
                float _BurnWidth;
                float _DissolveAmount;
                float _NoiseTiling;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.noiseUV = input.uv * _NoiseTiling;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.noiseUV).r;

                half threshold = _DissolveAmount;
                half clipVal = noise - threshold;

                clip(clipVal);

                half burnFactor = 1.0 - saturate(clipVal / _BurnWidth);
                half3 finalRGB = lerp(mainTex.rgb * _Color.rgb * input.color.rgb, _BurnColor.rgb, burnFactor);

                half finalAlpha = mainTex.a * _Color.a * input.color.a;

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
                float4 _Color;
                float4 _BurnColor;
                float _BurnWidth;
                float _DissolveAmount;
                float _NoiseTiling;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.noiseUV = input.uv * _NoiseTiling;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, input.noiseUV).r;

                half threshold = _DissolveAmount;
                half clipVal = noise - threshold;

                clip(clipVal);

                half burnFactor = 1.0 - saturate(clipVal / _BurnWidth);
                half3 finalRGB = lerp(mainTex.rgb * _Color.rgb * input.color.rgb, _BurnColor.rgb, burnFactor);

                half finalAlpha = mainTex.a * _Color.a * input.color.a;

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
