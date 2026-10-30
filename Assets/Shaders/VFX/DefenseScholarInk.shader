Shader "ProjectZombie/Defense3D/ScholarInk"
{
    Properties
    {
        _BaseColor("Jade", Color) = (.5,.82,.69,1)
        _InkColor("Ink", Color) = (.025,.045,.055,1)
        [HDR] _CoreColor("Ivory core", Color) = (1.8,1.65,1.15,1)
        _Progress("Lifetime", Range(0,1)) = 0
        _Phase("Cast / Release / Hit", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float4 _BaseColor, _InkColor, _CoreColor;
            float _Progress, _Phase;
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
            float Stroke(float2 p, float2 a, float2 b, float width, float aa)
            {
                float2 ab = b-a;
                float h = saturate(dot(p-a,ab)/dot(ab,ab));
                float taper = lerp(1.15,.5,h);
                return 1-smoothstep(width*taper-aa,width*taper+aa,length(p-a-ab*h));
            }
            half4 Frag(Varyings input):SV_Target
            {
                float2 p = input.uv*2-1;
                float r = length(p);
                float aa = max(fwidth(r),.002);
                float t = saturate(_Progress);
                float angle = atan2(p.y,p.x);
                float grain = sin(p.x*103+p.y*67)*sin(p.y*113-p.x*41);
                float brushRadius = .96 + .006*sin(angle*17) + .003*sin(angle*31);
                float ring = 1-smoothstep(.012-aa,.012+aa,abs(r-brushRadius));
                float inner = 1-smoothstep(.007-aa,.007+aa,abs(r-.69));
                // Abstract brush marks rather than font glyphs: no missing-glyph dependency on Android.
                float sector = angle*6.3661977;
                float2 glyph = float2((frac(sector+.5)-.5)*.23,r-.79);
                float variant = frac(floor(sector+.5)*.618034);
                float marks = Stroke(glyph,float2(-.045,.04),float2(.045,.04),.008,aa);
                marks = max(marks,Stroke(glyph,float2(-.025,.055),float2(-.025,-.045),.008,aa));
                marks = max(marks,Stroke(glyph,float2(-.045,-.015),float2(.05,-.015-variant*.035),.008,aa));
                marks = max(marks,Stroke(glyph,float2(.035,.04),float2(.035-variant*.06,-.05),.008,aa));
                float ink = max(Stroke(p,float2(-.31,.16),float2(.28,.23),.033,aa),
                    Stroke(p,float2(.06,.35),float2(-.1,-.32),.045,aa));
                ink = max(ink,Stroke(p,float2(-.25,-.06),float2(.28,-.19),.028,aa));
                ink = max(ink,Stroke(p,float2(-.2,.09),float2(-.24,-.2),.025,aa));
                ink = max(ink,Stroke(p,float2(.23,.17),float2(.25,-.3),.025,aa));
                ink = max(ink,Stroke(p,float2(-.26,-.3),float2(.25,-.3),.032,aa));
                ink = 0; // The ground only communicates range; calligraphy lives in the air ribbons.
                // The floor seal supports the airborne brush silhouette with a readable ornamental frame.
                float seal = max(ring, max(inner*.45, marks*.65));
                float glow = 0;
                float alpha = 0;
                if (_Phase < .5)
                {
                    float reveal = smoothstep(0,.8,t);
                    float trace = 1-smoothstep(t*6.283185-3.14,t*6.283185-3.04,angle);
                    float gatherRadius = lerp(.9,.24,t*t);
                    float gather = (1-smoothstep(.014,.014+aa,abs(r-gatherRadius))) *
                        pow(saturate(cos(angle*7+t*9)),12)*sin(t*3.14159);
                    seal *= trace*lerp(.25,1,reveal);
                    ink *= reveal;
                    glow = max(seal*(.3+.7*t*t),gather);
                    alpha = max(max(seal,ink*.8),gather)*lerp(.25,1,reveal);
                }
                else if (_Phase < 1.5)
                {
                    float ease = 1-pow(1-t,3);
                    float wave = 1-smoothstep(.018-aa,.018+aa,abs(r-lerp(.2,.96,ease)));
                    float flash = pow(saturate(1-t*5),2);
                    float decay = pow(1-t,1.5);
                    float dissolve = smoothstep(t-.8,t-.4,grain*.3+.4);
                    ink *= dissolve;
                    glow = max(wave*(1-t),max(seal*decay,ink*flash));
                    alpha = max(max(seal*decay,ink*decay),wave*(1-t));
                    // Full-radius outline is visible on the damage frame; the moving wave is only an aftershock.
                    alpha = max(alpha,ring*(1-t));
                }
                else
                {
                    float2 q = p / lerp(.35,1,1-pow(1-t,3));
                    float rays = max(Stroke(q,float2(-.8,-.25),float2(.8,.25),.04,aa),
                        Stroke(q,float2(-.2,.7),float2(.2,-.7),.035,aa));
                    float stamp = 1-smoothstep(.025-aa,.025+aa,abs(max(abs(q.x),abs(q.y))-.42));
                    rays = max(rays, stamp*.65);
                    glow = rays*pow(1-t,2);
                    alpha = glow;
                    ink = 0;
                }
                float3 color = lerp(_InkColor.rgb,_BaseColor.rgb,saturate(glow*2));
                color = lerp(color,_CoreColor.rgb,saturate(glow*pow(1-t,3)));
                return half4(color, saturate(alpha*_BaseColor.a*(_Phase < 1.5 ? .32 : 1))*(1-smoothstep(.98,1,r)));
            }
            ENDHLSL
        }
    }
}
