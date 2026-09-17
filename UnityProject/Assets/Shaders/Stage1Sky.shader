Shader "VectorRush/Stage1 City Atmosphere" { SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off Pass { HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 d:TEXCOORD0;};
V vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.d=a.p.xyz;return o;}
half4 frag(V i):SV_Target{float h=pow(1-saturate(normalize(i.d).y),2);return half4(lerp(float3(.005,.007,.015),float3(.038,.027,.042),h),1);}
ENDHLSL } } }
