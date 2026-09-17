Shader "VectorRush/Stage1 Local Rail Response"
{
 Properties { _Color("Source radiance",Color)=(0,1,1,1) }
 SubShader { Tags { "Queue"="Transparent-5" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Pass { Blend One One ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color;
 CBUFFER_END
 struct A { float4 pos:POSITION;float2 uv:TEXCOORD0;float3 normal:NORMAL; };
 struct V { float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;half fog:TEXCOORD3; };
 V vert(A a){V o;o.world=TransformObjectToWorld(a.pos.xyz);o.pos=TransformWorldToHClip(o.world);o.uv=a.uv;o.normal=TransformObjectToWorldNormal(a.normal);o.fog=ComputeFogFactor(o.pos.z);return o;}
 float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float Noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(a),Hash(a+float2(1,0)),f.x),lerp(Hash(a+float2(0,1)),Hash(a+1),f.x),f.y);}
 half4 frag(V i):SV_Target { float edge=pow(saturate(1-i.uv.y),2.6);float grazing=pow(1-saturate(abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world)))),1.4);float grain=.65+.35*Noise(i.world.xz*float2(3.7,5.2));float breakup=.55+.45*smoothstep(.2,.8,Noise(i.world.xz*.45));return half4(MixFogColor(_Color.rgb*edge*(.12+grazing*.72)*grain*breakup,0,i.fog),0);}
 ENDHLSL
 } }
}
