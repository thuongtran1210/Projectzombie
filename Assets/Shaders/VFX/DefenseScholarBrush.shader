Shader "ProjectZombie/Defense3D/ScholarBrush"
{
    Properties
    {
        _BaseColor("Jade", Color) = (.45,.9,.75,1)
        _Progress("Lifetime", Range(0,1)) = 0
        _Phase("Phase", Float) = 0
        _Kind("Ribbon / Fragment", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+110" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float _Progress, _Phase, _Kind;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                float u = input.uv.x;
                float v = input.uv.y * 2 - 1;
                float t = saturate(_Progress);
                float aa = max(fwidth(v),.015);
                float bristle = .065*sin(u*171+v*17)+.035*sin(u*317);
                float edge = 1-smoothstep(.83+bristle-aa,.96+bristle+aa,abs(v));
                float core = 1-smoothstep(.08,.2,abs(v+.15*sin(u*19)));
                float striation = .92+.08*sin(v*29+u*13);
                float tip = smoothstep(0,.04,u)*(1-smoothstep(.93,1,u));
                float reveal = _Phase < .5 ? 1-smoothstep(t*1.4-.12,t*1.4+.02,u) : 1;
                float decay = _Phase < .5 ? .55+.45*t : pow(1-t,1.4);
                float split = smoothstep(t*.8-.4,t*.8-.1,sin(u*63+v*21)*.22+.45);
                float3 ink = float3(.014,.025,.035);
                float3 jade = _BaseColor.rgb*1.25;
                float flash = _Phase < .5 ? .35*t*t : pow(saturate(1-t*3),2);
                float3 tint = lerp(ink,jade,1-smoothstep(.55,.85,abs(v)));
                tint = lerp(tint,float3(2.3,2.1,1.55),core*(.25+flash*.75));
                float alpha = edge*tip*reveal*decay*striation;
                if (_Phase > .5) alpha *= split;
                if (_Kind > .5)
                {
                    float drop = 1-smoothstep(.25,.85,abs(v)+abs(u-.45)*1.4);
                    alpha = drop*pow(1-t,1.2);
                    tint = lerp(ink,jade,step(.3,u)*step(u,.45));
                }
                return half4(tint,saturate(alpha*_BaseColor.a));
            }
            ENDHLSL
        }
    }
}
