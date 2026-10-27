Shader "ProjectZombie/Defense3D/CombatSlash"
{
    Properties { _BaseColor("Tint", Color) = (.39,1,.85,1) _Progress("Progress", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Progress;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input) { Varyings o; o.positionCS=TransformObjectToHClip(input.positionOS.xyz); o.uv=input.uv; return o; }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p=input.uv*2-1;
                float r=length(p);
                float a=atan2(p.x,p.y);
                float arc=saturate(1-abs(a)/2.04);
                float taper=smoothstep(0,.3,arc);
                float radius=.72+.1*_Progress+.04*sin(a*1.8);
                float d=abs(r-radius);
                float core=1-smoothstep(.012,.038*taper+.015,d);
                float glow=(1-smoothstep(.025,.16*taper+.025,d))*.3;
                float echo=(1-smoothstep(.008,.022,abs(r-radius+.13)))*.32;
                float alpha=(core+glow+echo)*taper*saturate((1-_Progress)*2.5)*smoothstep(0,.07,r);
                float3 color=lerp(_BaseColor.rgb,float3(.95,1,1),core*.9);
                return half4(color, saturate(alpha)*_BaseColor.a);
            }
            ENDHLSL
        }
    }
}
