Shader "ProjectZombie/Defense3D/IceLaserGlow"
{
    Properties { _Tint("Color",Color)=(.1,.8,1,1) _Progress("Pulse",Range(0,1))=0 }
    SubShader
    {
        Tags { "Queue"="Transparent+5" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float4 _Tint;
            float _Progress;
            CBUFFER_END
            struct A { float4 p:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 p:SV_POSITION; float2 uv:TEXCOORD0; };
            V Vert(A i)
            {
                V o; float3 center=TransformObjectToWorld(float3(0,0,0));
                float3 view=GetCameraPositionWS()-center;
                center+=view*rsqrt(max(dot(view,view),.0001))*.35;
                float3 right=float3(UNITY_MATRIX_I_V[0][0],UNITY_MATRIX_I_V[1][0],UNITY_MATRIX_I_V[2][0]);
                float3 up=float3(UNITY_MATRIX_I_V[0][1],UNITY_MATRIX_I_V[1][1],UNITY_MATRIX_I_V[2][1]);
                float2 size=float2(length(float3(UNITY_MATRIX_M[0][0],UNITY_MATRIX_M[1][0],UNITY_MATRIX_M[2][0])),length(float3(UNITY_MATRIX_M[0][1],UNITY_MATRIX_M[1][1],UNITY_MATRIX_M[2][1])));
                o.p=TransformWorldToHClip(center+right*i.p.x*size.x+up*i.p.y*size.y);o.uv=i.uv;return o;
            }
            half4 Frag(V i):SV_Target
            {
                float2 p=i.uv*2-1;float radius=length(p);float angle=atan2(p.y,p.x);
                float core=exp2(-radius*radius*32);
                float ring=pow(saturate(1-abs(radius-(.35+_Progress*.25))*20),2);
                float rays=pow(abs(cos(angle*3+_Progress*.8)),18)*pow(saturate(1-radius),2);
                float haze=pow(saturate(1-radius),3)*.3;
                float alpha=(core+ring*.6+rays*1.1+haze)*pow(1-_Progress,1.3);
                return half4(lerp(_Tint.rgb,float3(1,1,1),saturate(core+rays*.35))*1.6,alpha*_Tint.a);
            }
            ENDHLSL
        }
    }
}
