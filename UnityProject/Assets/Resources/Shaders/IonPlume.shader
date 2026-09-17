Shader "VectorRush/Ion Plume"
{
    Properties
    {
        [HDR] _Tint("Ion tint",Color)=(0.04,0.65,1.1,0.3)
        _Intensity("Radiance",Float)=1
        _Mode("0 plume, 1 core, 2 ring, 3 particle",Float)=0
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
            float4 _Tint;float _Intensity;float _Mode;
            CBUFFER_END
            Varyings vert(Attributes input)
            {
                Varyings output;float3 world=TransformObjectToWorld(input.positionOS.xyz);output.positionCS=TransformWorldToHClip(world);
                output.uv=input.uv;output.color=input.color;output.fog=ComputeFogFactor(output.positionCS.z);
                output.normalWS=TransformObjectToWorldNormal(input.normalOS);output.viewWS=GetWorldSpaceViewDir(world);return output;
            }
            half4 frag(Varyings input):SV_Target
            {
                float2 p=(input.uv-.5)*2;float radius=length(p);float shape;
                if(_Mode<.5)
                {
                    float along=saturate(input.uv.x);
                    float side=input.uv.y*2-1;
                    float facing=saturate(abs(dot(normalize(input.normalWS),normalize(input.viewWS))));
                    float drift=(sin(along*17-_Time.y*11)*.075+sin(along*29+_Time.y*7)*.035)*along;
                    float displaced=side-drift;
                    float crossSection=exp(-displaced*displaced*5.2);
                    float sideFade=1-smoothstep(.62,.98,abs(side));
                    // Carry readable density beyond the aperture, then dissolve smoothly.
                    // The Gaussian cross-section and zero-density borders remain unchanged.
                    float tail=(1-smoothstep(.68,1,along))*exp(-along*.45);
                    float cells=.84+.10*sin(along*24-_Time.y*12+side*4)+.06*sin(along*41+_Time.y*9-side*7);
                    // Density vanishes at sheet borders and along the tail. No edge-on
                    // brightness floor: the previous shell's floor revealed its triangles.
                    shape=crossSection*sideFade*tail*cells*pow(facing,.45);
                }
                else if(_Mode<1.5) shape=exp(-dot(p,p)*5.2)*(1-smoothstep(.40,1,radius));
                else if(_Mode<2.5) shape=pow(saturate(sin(input.uv.x*3.14159265)),.65);
                else shape=exp(-dot(p,p)*4.5)*(1-smoothstep(.3,1,radius));
                half3 color=_Tint.rgb*_Tint.a*input.color.rgb*input.color.a*_Intensity*shape;
                return half4(MixFogColor(color,half3(0,0,0),input.fog),0);
            }
            ENDHLSL
        }
    }
}
