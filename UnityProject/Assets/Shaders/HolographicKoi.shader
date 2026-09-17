Shader "VectorRush/Holographic Koi"
{
 Properties { _Color("Projection color",Color)=(1,.035,.008,1) _Power("Radiance",Float)=3 _Opacity("Density",Range(0,1))=.92 _Kind("Body 0 membrane 1 filament 2",Float)=0 }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+5"}
 Pass {
 Tags {"LightMode"="UniversalForward"}
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex Vert
 #pragma fragment Frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _Color;float _Power,_Opacity,_Kind;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;float3 local:TEXCOORD3;float fog:TEXCOORD4;};
 // Unity imports the head at positive X and the caudal tips at negative X.
 float3 Swim(float3 p){float lengthAxis=-p.x;float w=saturate((lengthAxis+12)/43);w=w*w*(3-2*w);p.z+=sin(_Time.y*1.9-lengthAxis*.13)*(.06+w*w*4.5);p.y+=sin(_Time.y*.95-lengthAxis*.10)*w*.8;return p;}
 V Vert(A i){V o;float3 p=Swim(i.positionOS.xyz);float3 derivative=(Swim(i.positionOS.xyz+float3(.02,0,0))-Swim(i.positionOS.xyz-float3(.02,0,0)))/.04;float3 normal=i.normalOS;normal.x-=derivative.y*normal.y+derivative.z*normal.z;o.local=i.positionOS.xyz;VertexPositionInputs v=GetVertexPositionInputs(p);o.positionCS=v.positionCS;o.world=v.positionWS;o.normal=TransformObjectToWorldNormal(normalize(normal));o.uv=i.uv;o.fog=ComputeFogFactor(v.positionCS.z);return o;}
 half4 Frag(V i):SV_Target {
 float3 n=normalize(i.normal);float3 view=normalize(GetWorldSpaceViewDir(i.world));float rim=pow(1-abs(dot(n,view)),2.5);float top=pow(saturate(n.y),3);
 float3 hot=float3(1,.52,.24);float3 color;float alpha=_Opacity;
 if(_Kind<.5){
  float2 q=float2(i.uv.x*48+floor(i.uv.y*30)*.5,i.uv.y*30);float2 cell=frac(q)-.5;float d=length(cell*float2(1,1.45));float aa=max(fwidth(d),.02);float scale=(1-smoothstep(.018,.018+aa,abs(d-.48)))*smoothstep(-.2,.25,cell.y);
  float detailFade=1-smoothstep(100,230,distance(GetCameraPositionWS(),i.world));
  float headFade=smoothstep(.06,.28,i.uv.x);
  float flow=.5+.5*sin(i.local.x*.19+i.local.y*.24-_Time.y*.28);
  float dorsal=smoothstep(-.15,.85,n.y);
  color=_Color.rgb*_Power*(.42+.12*flow+scale*.23*detailFade*headFade)+hot*(rim*.85+dorsal*.52);
  alpha*=.62+.30*rim;
 }else if(_Kind<1.5){color=lerp(_Color.rgb,float3(1,.21,.045),.4)*_Power+hot*rim*.4;alpha*=.7+.3*rim;}
 else if(_Kind<2.5){float warm=.5+.5*sin(i.local.x*.10+i.local.y*.14);color=lerp(_Color.rgb,float3(1,.27,.06),warm*.65)*_Power;}
 else{color=_Color.rgb;}
 color=MixFogColor(color,half3(0,0,0),i.fog);return half4(color,alpha);
 }
 ENDHLSL
 }
 }
}
