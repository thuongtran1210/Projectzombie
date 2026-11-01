Shader "ProjectZombie/Defense3D/PipeSmoke"
{
    Properties
    {
        _BaseColor("Tint", Color) = (0.65,0.7,0.48,1)
        _Phase("Ground / Smoke / Embers", Float) = 0
        _Progress("Session Progress", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+15" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Phase;
                float _Progress;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float2 seed:TEXCOORD1; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float2 seed:TEXCOORD1; };
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 p = input.positionOS.xyz;
                if (_Phase > 1.5)
                {
                    float t = frac(_Progress * 3 + input.seed.x);
                    p.y += t * 1.1;
                    p.x += sin(t * 4 + input.seed.x * 17) * .16;
                }
                output.positionCS = TransformObjectToHClip(p);
                output.uv = input.uv; output.seed = input.seed;
                return output;
            }
            float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);
            }
            half4 frag(Varyings input):SV_Target
            {
                float2 p = (input.uv-.5)*2;
                float d = length(p);
                float life = 1-smoothstep(.76,1,_Progress);
                float t = _Progress * 9;
                float alpha=0;
                float3 color=_BaseColor.rgb;
                if (_Phase < .5)
                {
                    float angle=atan2(p.y,p.x);
                    float edge=(smoothstep(.974,.98,d)-smoothstep(.993,1,d));
                    float marks=step(.65,sin(angle*12))*smoothstep(.84,.85,d)*(1-smoothstep(.89,.90,d));
                    float mist=(1-smoothstep(.65,1,d))*(.06+.08*noise(p*4+t*.12));
                    float shock=exp(-pow((d-saturate(_Progress/.075))*24,2))*(1-smoothstep(.05,.14,_Progress));
                    alpha = (edge*.58+marks*.32+mist+shock*.55)*life;
                    color=lerp(float3(.27,.39,.28),float3(1,.73,.26),saturate(edge+marks+shock));
                }
                else if (_Phase < 1.5)
                {
                    float n=noise(p*3+float2(t*.25,input.seed.x*12));
                    float lobes=.1*sin(atan2(p.y,p.x)*5+input.seed.x*6);
                    float shape=1-smoothstep(.58,.96,d+lobes+n*.13);
                    float folds=noise(p*5-float2(t*.18,t*.1)+input.seed.x*5);
                    float light=saturate(.65+p.y*.35-p.x*.2+folds*.2);
                    color=lerp(float3(.14,.24,.21),float3(.72,.76,.59),light);
                    color=lerp(color,float3(.85,.56,.22),(1-smoothstep(0,.13,_Progress))*.3);
                    alpha=shape*(.29+.14*folds)*life*smoothstep(0,.025,_Progress);
                }
                else
                {
                    float cycle=frac(_Progress*3+input.seed.x);
                    alpha=(1-smoothstep(.1,.8,d))*sin(cycle*PI)*life*.8;
                    color=float3(1,.65,.18);
                }
                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
