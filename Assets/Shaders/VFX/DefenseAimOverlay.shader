Shader "ProjectZombie/Defense3D/AimOverlay"
{
    Properties
    {
        [MainTexture] _MainTex("Ink Texture", 2D) = "white" {}
        [HDR] _BaseColor("Ink Tint Color", Color) = (0.05, 0.05, 0.08, 1.0)
        _AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.0
        [Enum(Line,0,Ring,1,Disc,2,Arrow,3,Triangle,4,Sector,5)] _Shape("Authored Shape", Float) = 0
        _EdgeWidth("Shape Edge Width", Range(0.005,0.2)) = 0.06
        _FillOpacity("Arrow Fill Opacity", Range(0,1)) = 0.18
        _UseVertexColor("UI Vertex Tint", Float) = 0
        _SectorCos("Sector Half Angle Cosine", Range(-1,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent+100" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "AimOverlayUnlit"
            // URP 2D and Forward renderers both draw this unlit pass.
            Tags { "LightMode" = "SRPDefaultUnlit" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _AlphaCutoff;
                float _Shape;
                float _EdgeWidth;
                float _FillOpacity;
                float _UseVertexColor;
                float _SectorCos;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * _BaseColor * input.color;
                if (_Shape > 0.5)
                {
                    float2 p = input.uv * 2.0 - 1.0;
                    float radius = length(p);
                    float aa = max(fwidth(radius), 0.001);
                    float coverage = 1.0;
                    if (_Shape < 1.5)
                        coverage = (1.0 - smoothstep(1.0-aa,1.0,radius)) * smoothstep(1.0-_EdgeWidth-aa,1.0-_EdgeWidth,radius);
                    else if (_Shape < 2.5)
                        coverage = 1.0 - smoothstep(1.0-aa,1.0,radius);
                    else if (_Shape > 4.5)
                    {
                        float directionCos = p.y / max(radius,0.0001);
                        float angularAA = max(fwidth(directionCos),0.001);
                        float angular = radius < 0.001 ? 1.0 : smoothstep(_SectorCos-angularAA,_SectorCos+angularAA,directionCos);
                        coverage = (1.0-smoothstep(1.0-aa,1.0,radius)) * angular;
                    }
                    else
                    {
                        float y = input.uv.y;
                        float x = abs(input.uv.x - 0.5);
                        float halfWidth = _Shape < 3.5 ? (y < 0.78 ? 0.28 : (1.0-y)*0.5/0.22) : (1.0-y)*0.5;
                        float edge = halfWidth-x;
                        float softness = max(fwidth(edge),0.001);
                        coverage = smoothstep(0.0,softness,edge);
                        if (_Shape < 3.5)
                            coverage *= lerp(_FillOpacity,1.0,1.0-smoothstep(_EdgeWidth,_EdgeWidth+softness,edge));
                    }
                    // Built-in quad meshes do not carry vertex colors. Shape materials own their tint.
                    finalColor = _BaseColor * lerp(half4(1,1,1,1),input.color,saturate(_UseVertexColor));
                    finalColor.a *= coverage;
                }

                clip(finalColor.a - _AlphaCutoff);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
