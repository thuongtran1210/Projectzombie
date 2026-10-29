Shader "ProjectZombie/Defense3D/ScholarSeal"
{
    Properties { _BaseColor("Mau an tran", Color) = (.5, .82, .69, .55) }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
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
                float2 p = input.uv * 2 - 1;
                float r = length(p);
                float aa = max(fwidth(r), .001);
                float outer = 1-smoothstep(.012-aa, .012+aa, abs(r-.965));
                float inner = 1-smoothstep(.006-aa, .006+aa, abs(r-.65));
                // Tám nét hướng tâm và tâm ấn cách điệu, giữ khoảng trống để dễ nhìn combat.
                float angle = atan2(p.y, p.x);
                float ticks = smoothstep(.96, .995, cos(angle*8)) * smoothstep(.72, .74, r) * (1-smoothstep(.84, .86, r));
                float2 a = abs(p);
                float cross = 1-smoothstep(.17-aa, .17+aa, min(a.x+a.y*5, a.y+a.x*5));
                float fill = max(max(outer, inner*.7), max(ticks, cross*.8));
                float ink = 1-smoothstep(.028-aa, .028+aa, abs(r-.965));
                float alpha = max(ink*.65, fill) * _BaseColor.a;
                float3 tint = lerp(_BaseColor.rgb, float3(.91,.77,.41), cross*.35);
                return half4(lerp(float3(.10,.067,.055), tint, fill), alpha);
            }
            ENDHLSL
        }
    }
}
