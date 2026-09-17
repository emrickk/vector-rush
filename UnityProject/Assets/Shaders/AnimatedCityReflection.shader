Shader "VectorRush/Animated City Reflection"
{
    Properties
    {
        _SurfaceMap("Glass and roughness mask", 2D) = "white" {}
        _City0("Approach reflection", Cube) = "" {}
        _City1("Bend reflection", Cube) = "" {}
        _City2("Exit reflection", Cube) = "" {}
        _Capture0("Approach capture position",Vector)=(0,0,0,0)
        _Capture1("Bend capture position",Vector)=(0,0,0,0)
        _Capture2("Exit capture position",Vector)=(0,0,0,0)
        _Strength("Reflection strength", Float) = 1
        _RoadSurface("Textured road response",Float)=0
        _Frames0("Aurora motion",2D)="black"{}
        _Frames1("Volt motion",2D)="black"{}
        _Frames2("Echo motion",2D)="black"{}
        _Frames3("Orbit motion",2D)="black"{}
        _Ticker("Scrolling headlines",2D)="black"{}
        _Titles("Landscape typography",2D)="black"{}
        _SignData("Existing sign world poses",2D)="black"{}
        _SignImages("Existing sign images",2DArray)=""{}
        _SignCount("Existing signs",Float)=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-20" }
        Pass
        {
            Name "Captured city glass"
            Tags { "LightMode"="UniversalForward" }
            Blend One One
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BillboardPlayback.hlsl"
            TEXTURE2D(_SignData);
            TEXTURE2D_ARRAY(_SignImages); SAMPLER(sampler_SignImages);
            TEXTURE2D(_SurfaceMap); SAMPLER(sampler_SurfaceMap);
            TEXTURECUBE(_City0); SAMPLER(sampler_City0);
            TEXTURECUBE(_City1); SAMPLER(sampler_City1);
            TEXTURECUBE(_City2); SAMPLER(sampler_City2);
            CBUFFER_START(UnityPerMaterial)
            float4 _SurfaceMap_ST;
            float4 _Capture0, _Capture1, _Capture2;
            float _Strength, _RoadSurface, _SignCount;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; float2 uv:TEXCOORD2; float fog:TEXCOORD3; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS;
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.uv=TRANSFORM_TEX(v.uv,_SurfaceMap);
                o.fog=ComputeFogFactor(p.positionCS.z);
                return o;
            }
            float3 Project(float3 direction,float3 position,float3 capture)
            {
                // Local box projection gives parallax to the static scene capture.
                float3 extents=float3(145,160,145);
                float3 bounds=capture+lerp(-extents,extents,step(0,direction));
                float3 distances=(bounds-position)/(direction+1e-5);
                float distance=max(1,min(distances.x,min(distances.y,distances.z)));
                return position+direction*distance-capture;
            }
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(a),Hash(a+float2(1,0)),f.x),lerp(Hash(a+float2(0,1)),Hash(a+1),f.x),f.y);}
            half3 ReflectSigns(float3 position,float3 direction,half roughness)
            {
                half3 result=0;
                [loop] for(int n=0;n<(int)_SignCount;n++)
                {
                    float4 origin=LOAD_TEXTURE2D(_SignData,int2(0,n));
                    float4 normalData=LOAD_TEXTURE2D(_SignData,int2(3,n));
                    float3 normal=normalData.xyz;
                    float4 style=LOAD_TEXTURE2D(_SignData,int2(4,n));
                    float denominator=dot(direction,normal);
                    if(denominator>=-.025)continue;
                    float distance=dot(origin.xyz-position,normal)/denominator;
                    if(distance<1||distance>160)continue;
                    float4 right=LOAD_TEXTURE2D(_SignData,int2(1,n));
                    float4 up=LOAD_TEXTURE2D(_SignData,int2(2,n));
                    float3 hit=position+direction*distance-origin.xyz;
                    float2 uv=float2(dot(hit,right.xyz)/right.w,dot(hit,up.xyz)/up.w)+.5;
                    if(any(uv<0)||any(uv>1))continue;
                    half edge=smoothstep(0,.08,min(min(uv.x,1-uv.x),min(uv.y,1-uv.y)));
                    result+=BillboardPlayback(uv,(int)origin.w,normalData.w,right.w/up.w,style.x,style.yz,3+roughness*2)*edge*2.4*style.w/1.3;
                }
                return result;
            }
            half4 Frag(Varyings i):SV_Target
            {
                half4 surface=SAMPLE_TEXTURE2D(_SurfaceMap,sampler_SurfaceMap,i.uv);
                half glass=smoothstep(.6,.8,surface.a);
                if(_RoadSurface<.5 && glass<.001)return 0;
                half3 view=normalize(GetWorldSpaceViewDir(i.positionWS));
                half3 normal=normalize(i.normalWS);
                half roughPatch=0;
                if(_RoadSurface>.5){roughPatch=smoothstep(.2,.85,Noise(i.positionWS.xz*.32));glass=1;normal=normalize(normal+float3(Noise(i.positionWS.xz*3)-.5,0,Noise(i.positionWS.zx*4)-.5)*.025);surface.a=.68+roughPatch*.27;}
                half3 direction=reflect(-view,normal);
                float3 delta0=i.positionWS-_Capture0.xyz;
                float3 delta1=i.positionWS-_Capture1.xyz;
                float3 delta2=i.positionWS-_Capture2.xyz;
                float3 distances=float3(dot(delta0.xz,delta0.xz),dot(delta1.xz,delta1.xz),dot(delta2.xz,delta2.xz));
                half locality=saturate((155-sqrt(min(distances.x,min(distances.y,distances.z))))/100);
                if(locality<=0)return 0;
                float3 weights=rcp(distances+900);
                weights/=dot(weights,1);
                half mip=1.0+(1-surface.a)*5;
                half3 light=SAMPLE_TEXTURECUBE_LOD(_City0,sampler_City0,Project(direction,i.positionWS,_Capture0.xyz),mip).rgb*weights.x;
                light+=SAMPLE_TEXTURECUBE_LOD(_City1,sampler_City1,Project(direction,i.positionWS,_Capture1.xyz),mip).rgb*weights.y;
                light+=SAMPLE_TEXTURECUBE_LOD(_City2,sampler_City2,Project(direction,i.positionWS,_Capture2.xyz),mip).rgb*weights.z;
                // Suppress broad low-radiance sky fill; retain luminous windows and signs.
                half peak=max(light.r,max(light.g,light.b));
                light*=smoothstep(.08,.6,peak);
                if(_RoadSurface>.5)light=light*.35+ReflectSigns(i.positionWS,direction,1-roughPatch);
                half fresnel=.08+.72*pow(1-saturate(abs(dot(normal,view))),4);
                if(_RoadSurface>.5)fresnel*=lerp(.15,1.0,roughPatch)*(.65+.35*Noise(i.positionWS.xz*7));
                half fog=ComputeFogIntensity(i.fog);
                return half4(light*glass*fresnel*_Strength*fog*locality,0);
            }
            ENDHLSL
        }
    }
}
