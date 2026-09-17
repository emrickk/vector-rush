Shader "VectorRush/Ion Plume Stage 2"
{
    Properties
    {
        [HDR] _Tint("Ion tint",Color)=(0.04,0.65,1.1,0.3)
        _Intensity("Radiance",Float)=1
        _Mode("0 plume, 1 core, 2 ring, 3 particle",Float)=0
        _FlowTime("Simulation flow time",Float)=0
        _Boost("Sustained boost",Range(0,1))=0
        _Seed("Nozzle variation",Float)=0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;half4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;half fog:TEXCOORD1;float3 normalWS:TEXCOORD2;float3 viewWS:TEXCOORD3; };
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;float _Intensity;float _Mode;float _FlowTime;float _Boost;float _Seed;
            CBUFFER_END
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }
            float noise21(float2 p)
            {
                float2 cell = floor(p), blend = frac(p);
                blend = blend * blend * (3.0 - 2.0 * blend);
                return lerp(lerp(hash21(cell), hash21(cell + float2(1, 0)), blend.x),
                            lerp(hash21(cell + float2(0, 1)), hash21(cell + 1), blend.x), blend.y);
            }
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 position = input.positionOS.xyz;
                if (_Mode < .5)
                {
                    float along = saturate(input.uv.x);
                    float flow = _FlowTime * (4.8 + _Boost * 3.6);
                    float envelope = sin(along * 3.14159265);
                    float2 bend = float2(
                        noise21(float2(along * 5.5 - flow, _Seed + 2.0)),
                        noise21(float2(along * 6.7 - flow * 1.13, _Seed + 8.7))) - .5;
                    float billow = noise21(float2(along * 11.0 - flow * 1.4, _Seed + 16.0)) - .5;
                    position.xy *= 1.0 + billow * .48 * envelope;
                    position.xy += bend * (.70 + _Boost * .25) * envelope;
                }
                float3 world = TransformObjectToWorld(position);
                output.positionCS = TransformWorldToHClip(world);
                output.uv = input.uv; output.color = input.color;
                output.fog = ComputeFogFactor(output.positionCS.z);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewWS = GetWorldSpaceViewDir(world);
                return output;
            }
            half4 frag(Varyings input):SV_Target
            {
                float2 p = (input.uv - .5) * 2;
                float radius = length(p), shape;
                if (_Mode < .5)
                {
                    float along = saturate(input.uv.x), side = p.y;
                    float flow = _FlowTime * (4.8 + _Boost * 3.6);
                    float n = noise21(float2(along * 5.5 - flow, side * 2.4 + _Seed));
                    float fine = noise21(float2(along * 23.0 - flow * 1.8, side * 6.0 + _Seed * 3.1));
                    float drift = (n - .5) * .55 * along;
                    float displaced = side - drift;
                    float crossSection = exp(-displaced * displaced * 2.5);
                    float sideFade = 1.0 - smoothstep(.58, .99, abs(side));
                    float tail = (1.0 - smoothstep(.57 + n * .12, .90 + fine * .10, along)) * exp(-along * .30);
                    float turbulence = .48 + n * .72 + (fine - .5) * .38 * along;
                    float facing = saturate(abs(dot(normalize(input.normalWS), normalize(input.viewWS))));
                    // Facing attenuation hides sheet edges while the soft center keeps
                    // the paired nozzles readable from the normal chase angle.
                    shape = crossSection * sideFade * tail * turbulence * pow(facing, .55);
                }
                else if (_Mode < 1.5) shape = exp(-dot(p,p) * 5.2) * (1.0 - smoothstep(.40,1.0,radius));
                else if (_Mode < 2.5) shape = pow(saturate(sin(input.uv.x * 3.14159265)), .65);
                else shape = exp(-dot(p,p) * 4.5) * (1.0 - smoothstep(.3,1.0,radius));
                half3 color = _Tint.rgb * _Tint.a * input.color.rgb * input.color.a * _Intensity * shape;
                return half4(MixFogColor(color,half3(0,0,0),input.fog),0);
            }
            ENDHLSL
        }
    }
}
