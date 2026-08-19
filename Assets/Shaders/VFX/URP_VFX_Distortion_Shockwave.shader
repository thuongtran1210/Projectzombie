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
            Name "VFX_Distortion_Shockwave"
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
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // 1. Đọc texture mask và normal
                half4 maskTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv);

                // 2. Tính toán hướng đẩy lệch từ tâm UV (0.5, 0.5)
                float2 centerOffset = input.uv - float2(0.5, 0.5);
                float distFromCenter = length(centerOffset) * 2.0;

                // Tính vòng tròn sóng nổ (Ring Mask)
                half ringIntensity = smoothstep(1.0 - _WaveWidth, 1.0, distFromCenter) * (1.0 - smoothstep(1.0, 1.0 + _WaveWidth * _Falloff, distFromCenter));
                ringIntensity *= maskTex.r * input.color.a;

                // Vector bóp méo không gian
                float2 distortVector = normalize(centerOffset + (normalSample.rg - 0.5) * 0.5) * _DistortionStrength * ringIntensity;

                // 3. Lấy mẫu màu màn hình nền đệm đã bị làm lệch (Distorted Scene Color)
                float2 finalScreenUV = screenUV + distortVector;
                half3 sceneColor = SampleSceneColor(finalScreenUV);

                // 4. Pha trộn hào quang màu viền sóng
                half3 finalRGB = sceneColor + _TintColor.rgb * ringIntensity * _TintColor.a;
                half finalAlpha = saturate(ringIntensity * 2.0);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}
