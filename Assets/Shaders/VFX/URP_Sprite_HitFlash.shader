Shader "ProjectZombie/Sprite_HitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _Flip ("Flip", Vector) = (1, 1, 1, 1)

        [Header(Hit Flash Settings)]
        [PerRendererData] _FlashColor ("Flash Color (Trắng / Vàng Kim)", Color) = (1, 1, 1, 1)
        [PerRendererData] _FlashAmount ("Flash Intensity Amount (0 - 1)", Range(0, 1)) = 0.0
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
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RendererColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Flip)
                UNITY_DEFINE_INSTANCED_PROP(float4, _FlashColor)
                UNITY_DEFINE_INSTANCED_PROP(float,  _FlashAmount)
            UNITY_INSTANCING_BUFFER_END(Props)

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 flip = UNITY_ACCESS_INSTANCED_PROP(Props, _Flip);
                input.positionOS.xy *= flip.xy;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * UNITY_ACCESS_INSTANCED_PROP(Props, _RendererColor);

                #ifdef PIXELSNAP_ON
                    output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * input.color;

                float flashAmount = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashAmount);
                float4 flashColor = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashColor);

                finalColor.rgb = lerp(finalColor.rgb, flashColor.rgb * finalColor.a, saturate(flashAmount));

                return finalColor;
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RendererColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Flip)
                UNITY_DEFINE_INSTANCED_PROP(float4, _FlashColor)
                UNITY_DEFINE_INSTANCED_PROP(float,  _FlashAmount)
            UNITY_INSTANCING_BUFFER_END(Props)

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 flip = UNITY_ACCESS_INSTANCED_PROP(Props, _Flip);
                input.positionOS.xy *= flip.xy;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * UNITY_ACCESS_INSTANCED_PROP(Props, _RendererColor);

                #ifdef PIXELSNAP_ON
                    output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * input.color;

                float flashAmount = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashAmount);
                float4 flashColor = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashColor);

                finalColor.rgb = lerp(finalColor.rgb, flashColor.rgb * finalColor.a, saturate(flashAmount));

                return finalColor;
            }
            ENDHLSL
        }
    }
}
