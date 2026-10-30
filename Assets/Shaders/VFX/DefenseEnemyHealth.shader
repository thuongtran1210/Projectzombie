Shader "ProjectZombie/Defense3D/EnemyHealth"
{
    Properties
    {
        _Ratio("Health", Range(0,1)) = 1
        _FillColor("Fill", Color) = (.85,.18,.14,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+150" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float4 _FillColor;
            float _Ratio;
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
                float2 uv = input.uv;
                float inside = step(.025,uv.x)*step(uv.x,.975)*step(.16,uv.y)*step(uv.y,.84);
                float fill = step((uv.x-.025)/.95,saturate(_Ratio))*inside;
                float3 color = lerp(float3(.025,.035,.04),float3(.12,.035,.025),inside);
                color = lerp(color,_FillColor.rgb*(.8+.3*uv.y),fill);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
