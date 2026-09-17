Shader "VectorRush/Koi Road Reflection"
{
 Properties{_Fish("Live fish projection",2D)="black"{} _Center("Center",Vector)=(0,0,0,0) _Right("Right",Vector)=(1,0,0,0) _Up("Up",Vector)=(0,1,0,0) _Normal("Normal",Vector)=(0,0,1,0) _Size("Capture size",Vector)=(80,40,0,0) _Strength("Strength",Float)=.22}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-5"}
 Pass{Tags{"LightMode"="UniversalForward"} Blend One One ZWrite Off Offset -2,-2
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_Fish);SAMPLER(sampler_Fish);
 CBUFFER_START(UnityPerMaterial)
 float4 _Center,_Right,_Up,_Normal,_Size;float _Strength;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
 struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;};
 V Vert(A a){V v;VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);v.positionCS=p.positionCS;v.world=p.positionWS;v.normal=TransformObjectToWorldNormal(a.normalOS);v.uv=a.uv;v.fog=ComputeFogFactor(p.positionCS.z);return v;}
 half4 Frag(V v):SV_Target{float3 view=normalize(GetWorldSpaceViewDir(v.world));float3 n=normalize(v.normal+float3(sin(v.world.z*4.3+v.world.x*1.8),0,cos(v.world.x*5.1))*.014);float3 ray=reflect(-view,n);float denom=dot(ray,_Normal.xyz);if(abs(denom)<.02)return 0;float distance=dot(_Center.xyz-v.world,_Normal.xyz)/denom;if(distance<0||distance>240)return 0;float3 hit=v.world+ray*distance-_Center.xyz;float2 uv=float2(dot(hit,_Right.xyz)/_Size.x,dot(hit,_Up.xyz)/_Size.y)+.5;if(any(uv<0)||any(uv>1))return 0;float2 jitter=float2(sin(v.world.x*12+v.world.z*7),cos(v.world.z*9))*.003;half3 light=SAMPLE_TEXTURE2D(_Fish,sampler_Fish,uv+jitter).rgb*.4;light+=(SAMPLE_TEXTURE2D(_Fish,sampler_Fish,uv+jitter+float2(.006,0)).rgb+SAMPLE_TEXTURE2D(_Fish,sampler_Fish,uv+jitter-float2(.006,0)).rgb+SAMPLE_TEXTURE2D(_Fish,sampler_Fish,uv+jitter+float2(0,.012)).rgb+SAMPLE_TEXTURE2D(_Fish,sampler_Fish,uv+jitter-float2(0,.012)).rgb)*.15;float rough=.55+.45*sin(v.world.z*17.8+sin(v.world.x*8));float edge=smoothstep(0,.07,v.uv.x)*smoothstep(0,.07,1-v.uv.x)*smoothstep(0,.08,v.uv.y)*smoothstep(0,.08,1-v.uv.y);return half4(MixFogColor(light*_Strength*rough*edge,half3(0,0,0),v.fog),0);}
 ENDHLSL
 }}
}
