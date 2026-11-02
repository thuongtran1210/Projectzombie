Shader "ProjectZombie/Defense3D/TowerLaser"
{
    Properties { _BaseColor("Tint", Color) = (1,1,1,1) _Progress("Pulse", Range(0,1))=0 _Length("Length", Float)=3 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Progress, _Length;
            CBUFFER_END
            struct A { float4 p:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            struct V { float4 p:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; };
            V Vert(A i) { V o; o.p=TransformObjectToHClip(i.p.xyz);o.color=i.color*_BaseColor;o.uv=i.uv;return o; }
            half4 Frag(V i):SV_Target
            {
                float edge=abs(i.uv.y*2-1);
                float body=pow(saturate(1-edge),1.4);
                float core=pow(saturate(1-edge),12);
                float pulse=pow(saturate(cos((i.uv.x*_Length*1.6-_Progress*6)*6.283)),10);
                float fracture=pow(saturate(1-abs(frac(i.uv.x*_Length*2.5+sin(i.uv.x*19-_Progress*10)*.2)-.5)*16),3);
                float tips=smoothstep(0,.025,i.uv.x)*smoothstep(0,.025,1-i.uv.x);
                float3 color=i.color.rgb*(1+1.3*pulse)+float3(.55,.9,1)*core*.8;
                return half4(color,i.color.a*(body+fracture*body*.5)*tips);
            }
            ENDHLSL
        }
    }
}
