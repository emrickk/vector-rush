Shader "VectorRush/Rain and Spray"
{
 Properties { _Mist("Mist",Float)=0 }
 SubShader { Tags { "Queue"="Transparent+10" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
 Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _Mist;
 CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;half fog:TEXCOORD1;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.uv=a.uv;o.color=a.color;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target {float2 p=i.uv*2-1;float mist=exp(-dot(p,p)*4)*saturate(1-dot(p,p));float streak=pow(saturate(1-abs(p.x)),2)*saturate(1-abs(p.y));return half4(MixFog(i.color.rgb,i.fog),i.color.a*lerp(streak,mist,_Mist));}
 ENDHLSL
 } }
}
