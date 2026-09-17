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
 V Vert(A i){V o;float3 p=i.positionOS.xyz;float lengthAxis=-p.x;float weight=saturate((lengthAxis+12)/43);float wave=sin(_Time.y*2.8-lengthAxis*.19);p.z+=wave*(.15+weight*weight*6.0);p.y+=sin(_Time.y*1.4-lengthAxis*.13)*weight*1.2;o.local=p;VertexPositionInputs v=GetVertexPositionInputs(p);o.positionCS=v.positionCS;o.world=v.positionWS;o.normal=TransformObjectToWorldNormal(i.normalOS);o.uv=i.uv;o.fog=ComputeFogFactor(v.positionCS.z);return o;}
 half4 Frag(V i):SV_Target {
 float3 n=normalize(i.normal);float3 view=normalize(GetWorldSpaceViewDir(i.world));float rim=pow(1-abs(dot(n,view)),2.5);float top=pow(saturate(n.y),3);
 float3 hot=float3(1,.78,.65);float3 color;float alpha=_Opacity;
 if(_Kind<.5){
  float2 q=float2(i.uv.x*34+floor(i.uv.y*26)*.5,i.uv.y*26);float2 cell=frac(q)-.5;float d=length(cell*float2(1,1.35));float aa=max(fwidth(d),.015);float scale=1-smoothstep(.025,.025+aa,abs(d-.48));
  float variation=.86+.14*sin(floor(q.x)*4.7+floor(q.y)*2.6);
  color=_Color.rgb*(_Power*(.32+scale*1.8+rim*.5)*variation)+hot*(scale*.35+rim*.9+top*.6);
  float bands=.72+.28*sin(i.local.x*2.8+_Time.y*1.1);alpha*=bands*(.5+.5*rim);
  // Sparse moving light points reward proximity without introducing broad flicker.
  float dotlight=pow(saturate(1-length(cell*5)),8)*(.5+.5*sin(floor(q.x)*1.7+floor(q.y)*.4-_Time.y*.45));color+=hot*dotlight*1.2;
 }else if(_Kind<1.5){color=_Color.rgb*_Power+hot*rim*.2;alpha*=.65+.35*rim;}
 else if(_Kind<2.5){color=_Color.rgb*_Power*(.8+.2*sin(i.local.x*.23-_Time.y*.7));}
 else{color=_Color.rgb;}
 color=MixFogColor(color,half3(0,0,0),i.fog);return half4(color,alpha);
 }
 ENDHLSL
 }
 }
}
