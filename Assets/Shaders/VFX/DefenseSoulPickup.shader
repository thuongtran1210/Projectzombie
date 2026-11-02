Shader "ProjectZombie/Defense3D/SoulPickup"
{
    Properties
    {
        _Tint("Soul tint",Color)=(.38,.9,.79,1)
        _AnimTime("Session animation time",Float)=0
        _Phase("Instance phase",Float)=0
        _GlowStrength("Glow strength",Range(0,2))=.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;
            float _AnimTime,_Phase,_GlowStrength;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
            struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            V Vert(A input)
            {
                V o;
                float3 center=TransformObjectToWorld(float3(0,0,0));
                center.y+=.24+sin(_AnimTime*2.3+_Phase)*.055;
                float3 right=UNITY_MATRIX_I_V._m00_m10_m20;
                float3 up=UNITY_MATRIX_I_V._m01_m11_m21;
                float sx=length(unity_ObjectToWorld._m00_m10_m20);
                float sy=length(unity_ObjectToWorld._m01_m11_m21);
                float3 world=center+right*input.positionOS.x*sx+up*input.positionOS.y*sy;
                o.positionCS=TransformWorldToHClip(world);o.uv=input.uv;return o;
            }
            half4 Frag(V input):SV_Target
            {
                float2 p=input.uv;
                float sway=sin(_AnimTime*2.1+_Phase)*.018;
                float head=length((p-float2(.5,.61))/float2(1,1.07))-.185;
                float y=saturate((p.y-.15)/.46);
                float spine=.5+.105*(1-y)*(1-y)+sway*(1-y);
                float radius=.018+.155*smoothstep(0,1,y);
                float body=max(abs(p.x-spine)-radius,max(.15-p.y,p.y-.61));
                float field=min(head,body);
                float aa=max(fwidth(field),.0015);
                float solid=1-smoothstep(-aa,aa,field);
                float edge=min(min(p.x,1-p.x),min(p.y,1-p.y));
                float halo=exp(-max(field,0)*18)*(1-solid)*_GlowStrength*smoothstep(0,.12,edge);
                float inner=saturate(-field/.15);
                float breathe=.96+.04*sin(_AnimTime*2.8+_Phase);
                float3 color=lerp(_Tint.rgb,float3(.96,1,.95),inner*.9)*breathe*1.12;
                float left=length((p-float2(.443,.625))/float2(.017,.026));
                float right=length((p-float2(.557,.625))/float2(.017,.026));
                float eyes=1-smoothstep(.7,1.1,min(left,right));
                color=lerp(color,float3(.075,.24,.23),eyes*.85);
                float alpha=solid*(.76+inner*.20)*_Tint.a;
                float3 glow=_Tint.rgb*halo*(.9+.1*sin(_AnimTime*2.8+_Phase));
                clip(max(alpha,halo)-.003);
                return half4(color*alpha+glow*_Tint.a,alpha);
            }
            ENDHLSL
        }
    }
}
