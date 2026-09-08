Shader "ProjectZombie/VFX/SkillIndicator_Sector"
{
    Properties
    {
        [MainColor] _TintColor ("Tint Color", Color) = (0.2, 0.85, 1.0, 0.75)
        _BorderColor ("Border Glow Color", Color) = (0.6, 0.95, 1.0, 0.95)
        _ArcAngle ("Arc Angle (Degrees)", Range(1, 360)) = 90
        _BorderWidth ("Border Width", Range(0.01, 0.3)) = 0.06
        _InnerAlpha ("Inner Fill Alpha", Range(0.0, 1.0)) = 0.4
        _EdgeFeather ("Edge Feathering", Range(0.001, 0.05)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SectorArcPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TintColor;
                float4 _BorderColor;
                float _ArcAngle;
                float _BorderWidth;
                float _InnerAlpha;
                float _EdgeFeather;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Chuyển đổi UV về hệ tọa độ gốc ở chân nhân vật (0, 0.5) hoặc tâm (0, 0)
                // Giả định Sprite Quad có gốc quay chuẩn hướng trục +X (hoặc UV từ -1 đến 1)
                float2 centeredUV = (input.uv - 0.5) * 2.0; // [-1, 1]

                // Bán kính từ gốc
                float dist = length(centeredUV);
                if (dist > 1.0)
                {
                    discard;
                }

                // Góc polar tính từ trục X dương
                float angle = atan2(centeredUV.y, centeredUV.x) * 57.29578; // Deg
                float halfArc = _ArcAngle * 0.5;

                // Kiểm tra góc nằm trong cung quạt [-halfArc, +halfArc]
                float angleDist = abs(angle);
                if (angleDist > halfArc)
                {
                    discard;
                }

                // Tính toán độ mềm viền cung quạt (Angular feather)
                float angleFeather = smoothstep(halfArc, halfArc - (_EdgeFeather * 100.0), angleDist);

                // Tính toán độ mềm viền tròn ngoài (Radial outer feather)
                float radialFeather = smoothstep(1.0, 1.0 - _EdgeFeather, dist);

                // Hiệu ứng viền phát sáng (Border Glow)
                float isBorderRadial = smoothstep(1.0 - _BorderWidth, 1.0, dist);
                float isBorderAngular = smoothstep(halfArc - (_BorderWidth * 50.0), halfArc, angleDist);
                float borderFactor = max(isBorderRadial, isBorderAngular);

                half4 finalColor = lerp(_TintColor * float4(1, 1, 1, _InnerAlpha), _BorderColor, borderFactor);
                finalColor.a *= (angleFeather * radialFeather * input.color.a);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
