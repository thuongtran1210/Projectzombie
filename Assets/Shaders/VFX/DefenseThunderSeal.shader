Shader "ProjectZombie/Defense3D/ThunderSeal"
{
    Properties { _Tint("Tint",Color)=(.25,.7,1,1) _Progress("Progress",Range(0,1))=0 _Phase("Phase",Float)=0 _Intensity("Energy",Float)=1 }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float4 _Tint;float _Progress,_Phase,_Intensity;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
            float Ring(float r,float center,float width){return 1-smoothstep(width*.3,width,abs(r-center));}
            half4 Frag(V i):SV_Target
            {
                float2 q=i.uv*2-1;float r=length(q),a=atan2(q.y,q.x);float p=saturate(_Progress);
                float outer=Ring(r,.97,.014),inner=Ring(r,.72,.009),center=Ring(r,.26,.012);
                float runes=step(.83,abs(cos(a*12)))*step(.79,r)*step(r,.9);
                float spokes=step(.97,abs(cos(a*4)))*step(.28,r)*step(r,.68);
                float seal=outer+inner*.7+center+runes*.8+spokes*.5;
                float shock=Ring(r,.1+p*.88,.055)*(1-p);
                float cracks=pow(saturate(cos(a*17+sin(r*36)*.9)),36)*step(.12,r)*step(r,.92)*(1-p);
                float burst=exp2(-r*r*18)*pow(1-p,4)*1.5;
                float alpha=(_Phase<.5?seal*(.5+p*.5)+.025*step(r,.95):shock+cracks*.7+burst+seal*.22)*_Intensity;
                return half4(lerp(_Tint.rgb,float3(.85,.93,1),saturate(burst)),alpha*_Tint.a);
            }
            ENDHLSL
        }
    }
}
