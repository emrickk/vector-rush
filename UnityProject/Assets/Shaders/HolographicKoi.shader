Shader "VectorRush/Holographic Koi"
{
 Properties { _Color("Projection color",Color)=(1,.035,.008,1) _Power("Radiance",Float)=3 _Opacity("Density",Range(0,1))=.6 _Kind("Body 0 membrane 1 filament 2 anatomy 3",Float)=0 }
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
 float4 _Color; float _Power,_Opacity,_Kind;
 CBUFFER_END
 struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;float3 local:TEXCOORD3;float fog:TEXCOORD4;float4 color:TEXCOORD5;};
 // Imported anatomy has positive X at the head. Fin flexibility is zero at
 // attachment and increases along the authored membrane, shared by its veins.
 float3 Swim(float3 p,float2 fin)
 {
  float lengthAxis=-p.x;
  float w=saturate((lengthAxis+18)/59);w=w*w*(3-2*w);
  float beat=_Time.y*1.75-lengthAxis*.105;
  p.z+=sin(beat)*(.06+w*w*4.0);
  p.y+=sin(beat*.55)*w*.55;
  float lag=fin.x*fin.x;
  p.z+=sin(beat-fin.x*1.35+fin.y*3)*lag*1.0;
  p.y+=sin(beat-fin.x*1.1+fin.y*3)*lag*.65;
  return p;
 }
 V Vert(A i)
 {
  V o;float3 p=Swim(i.positionOS.xyz,i.color.rg);
  float3 slope=(Swim(i.positionOS.xyz+float3(.02,0,0),i.color.rg)-Swim(i.positionOS.xyz-float3(.02,0,0),i.color.rg))/.04;
  float3 n=i.normalOS;n.x-=slope.y*n.y+slope.z*n.z;
  VertexPositionInputs v=GetVertexPositionInputs(p);o.positionCS=v.positionCS;o.world=v.positionWS;
  o.normal=TransformObjectToWorldNormal(normalize(n));o.local=i.positionOS.xyz;o.uv=i.uv;o.color=i.color;o.fog=ComputeFogFactor(v.positionCS.z);return o;
 }
 half4 Frag(V i):SV_Target
 {
  float3 n=normalize(i.normal);float3 view=normalize(GetWorldSpaceViewDir(i.world));
  float rim=pow(1-abs(dot(n,view)),2.2);float crest=pow(saturate(n.y),9);
  float3 hot=float3(1,.76,.55);float3 color;float alpha=_Opacity;
  if(_Kind<.5)
  {
   // Offset rows of overlapping arcs: single open scale edges, not circles.
   float2 q=float2(i.uv.x*38,i.uv.y*30);q.x+=floor(q.y)*.5;
   float2 cell=frac(q);float arc=.72-1.25*(cell.y-.5)*(cell.y-.5);
   float edge=abs(cell.x-arc);float aa=max(fwidth(edge),.012);
   float scale=1-smoothstep(.018,.018+aa,edge);
   float head=smoothstep(.17,.30,i.uv.x)*(1-smoothstep(.93,1,i.uv.x));
   float distanceFade=1-smoothstep(150,340,distance(GetCameraPositionWS(),i.world));
   float glint=.35+.65*pow(.5+.5*sin(floor(q.x)*2.17+floor(q.y)*3.71),4);
   float density=.34+.20*saturate((arc-cell.x)*2);
   color=_Color.rgb*_Power*(density+scale*.35*head*distanceFade);
   color+=hot*(crest*3.3+rim*1.15+scale*head*distanceFade*glint*(.45+crest*1.2));
   float speck=pow(saturate(1-length((cell-float2(.3,.3))*10)),5);
   color+=hot*speck*head*.55;
   alpha*=.64+.30*rim;
  }
  else if(_Kind<1.5)
  {
   float edge=pow(saturate(sin(i.uv.y*PI)),.55);
   float taper=1-smoothstep(.88,1,i.uv.x);
   float veins=pow(.5+.5*sin(i.uv.y*29+i.uv.x*5+sin(i.uv.x*9+i.color.g*8)),8);
   color=_Color.rgb*_Power*(.48+.34*veins)+hot*(rim*.75+veins*.35);
   alpha*=edge*taper*(.65+.35*rim);
  }
  else if(_Kind<2.5)
  {
   float gleam=.5+.5*sin(i.local.x*.12+i.local.y*.3-_Time.y*.25);
   color=lerp(_Color.rgb,hot,gleam*.5)*_Power;
   alpha*=1-smoothstep(.82,1,i.color.r);
  }
  else color=_Color.rgb;
  // A far-side eye should not appear as an extra eye on the visible cheek.
  if((_Kind>=3||(_Kind>1.5&&i.local.x>13))&&abs(i.local.z)>1.5)
  {
   float3 localView=TransformWorldToObjectDir(view);alpha*=smoothstep(-.12,.25,localView.z*sign(i.local.z));
  }
  return half4(MixFogColor(color,half3(0,0,0),i.fog),alpha);
 }
 ENDHLSL
 }
 }
}
