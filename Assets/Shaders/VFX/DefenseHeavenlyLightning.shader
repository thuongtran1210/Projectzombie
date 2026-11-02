Shader "ProjectZombie/Defense3D/HeavenlyLightning"
{
    Properties { _Progress("Progress",Range(0,1))=0 _Intensity("Energy",Float)=1 }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float _Progress,_Intensity;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.color=i.color;return o;}
            half4 Frag(V i):SV_Target
            {
                float edge=abs(i.uv.y*2-1);float core=pow(saturate(1-edge),9);float halo=pow(saturate(1-edge),.8);
                float pulse=.8+.35*cos(i.uv.x*55-_Progress*38);
                float3 color=lerp(float3(.2,.12,1),i.color.rgb,saturate(1-edge*edge));color=lerp(color,float3(1,1,.96),core);
                return half4(color*(1+core*.8),i.color.a*halo*pulse*saturate(_Intensity));
            }
            ENDHLSL
        }
    }
}
