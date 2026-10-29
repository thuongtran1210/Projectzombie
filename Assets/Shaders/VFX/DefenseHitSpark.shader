Shader "ProjectZombie/Defense3D/HitSpark"
{
    Properties
    {
        _BaseColor("Mau tia", Color) = (1, .86, .58, 1)
        _Progress("Tien do", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float _Progress;
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
                // Bốn tia chính và tia chéo ngắn, viền nâu mực; không glow hoặc texture bổ sung.
                float2 a = abs(p);
                float2 q = abs(float2(p.x+p.y, p.x-p.y) * .70710678);
                float primary = min(a.x + a.y * 5, a.y + a.x * 5);
                float diagonal = min(q.x + q.y * 6, q.y + q.x * 6) * 1.5;
                float shape = min(primary, diagonal);
                float aa = max(fwidth(shape), .005);
                float outline = 1 - smoothstep(.84-aa, .84+aa, shape);
                float fill = 1 - smoothstep(.66-aa, .66+aa, shape);
                float3 tint = lerp(_BaseColor.rgb, float3(1, .97, .86), saturate(1-length(p)*2));
                float3 color = lerp(float3(.10, .067, .055), tint, fill);
                return half4(color, outline * _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
