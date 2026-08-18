Shader "ProjectZombie/SpriteOutline2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)

        [Header(Outline Settings)]
        [PerRendererData] _OutlineColor ("Outline Color", Color) = (1, 1, 1, 0)
        [PerRendererData] _OutlineThickness ("Outline Thickness", Range(0, 10)) = 2.0
        [PerRendererData] _OutlineGlow ("Outline Glow Intensity", Range(1, 5)) = 1.2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineThickness;
                float _OutlineGlow;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;

                #ifdef PIXELSNAP_ON
                output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float4 spriteColor = mainTexColor * input.color;

                // Nếu pixel hiện tại có alpha > 0.05, vẽ pixel gốc của Sprite
                if (spriteColor.a > 0.05)
                {
                    // Premultiplied alpha output
                    return float4(spriteColor.rgb * spriteColor.a, spriteColor.a);
                }

                // Nếu không có outline hoặc thickness <= 0 thì trả về trong suốt
                if (_OutlineColor.a <= 0.01 || _OutlineThickness <= 0.01)
                {
                    return float4(0, 0, 0, 0);
                }

                // Lấy mẫu 8 hướng xung quanh để phát hiện viền mép (Outer Outline)
                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;
                
                float maxNeighborAlpha = 0.0;
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offset.x, 0)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv - float2(offset.x, 0)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, offset.y)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv - float2(0, offset.y)).a);

                // Thêm 4 hướng đường chéo cho viền mượt mà (Diagonal sampling)
                float2 diagOffset = offset * 0.7071; // 1 / sqrt(2)
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(diagOffset.x, diagOffset.y)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-diagOffset.x, diagOffset.y)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(diagOffset.x, -diagOffset.y)).a);
                maxNeighborAlpha = max(maxNeighborAlpha, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-diagOffset.x, -diagOffset.y)).a);

                if (maxNeighborAlpha > 0.05)
                {
                    float outlineAlpha = _OutlineColor.a * maxNeighborAlpha;
                    float3 outlineRgb = _OutlineColor.rgb * _OutlineGlow;
                    return float4(outlineRgb * outlineAlpha, outlineAlpha);
                }

                return float4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
    Fallback "Sprites/Default"
}
