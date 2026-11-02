Shader "ProjectZombie/Defense3D/StructureToon"
{
    Properties { _BaseColor("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            struct A { float4 p:POSITION; float3 n:NORMAL; };
            struct V { float4 p:SV_POSITION; float3 n:TEXCOORD0; };
            V Vert(A i) { V o; o.p=TransformObjectToHClip(i.p.xyz); o.n=TransformObjectToWorldNormal(i.n); return o; }
            half4 Frag(V i):SV_Target
            {
                float light = dot(normalize(i.n), normalize(float3(-.4,.85,-.5)));
                return half4(_BaseColor.rgb * (light > .3 ? 1.12 : light > -.2 ? .85 : .57), 1);
            }
            ENDHLSL
        }
    }
}
