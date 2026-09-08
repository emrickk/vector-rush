Shader "VectorRush/Ocean"
{
    Properties { _BaseColor("Ocean tint",Color)=(0.015,0.16,0.23,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float fog:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            V vert(A v){V o;o.world=TransformObjectToWorld(v.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            half4 frag(V i):SV_Target
            {
                float2 p=i.world.xz;
                float t=_Time.y;
                float footprint=max(length(ddx(p)),length(ddy(p)));
                float detail=rcp(1+footprint*footprint*.16);
                float a=sin(dot(p,float2(.077,.113))+t*.45);
                float b=sin(dot(p,float2(-.19,.23))-t*.65+a*.7);
                float c=cos(dot(p,float2(.4,-.12))+t*.3+b*.6);
                float3 n=normalize(float3((a*.042+b*.018)*detail,1,(b*.035+c*.015)*detail));
                float3 view=normalize(_WorldSpaceCameraPos-i.world);
                Light sun=GetMainLight();
                float fres=pow(1-saturate(dot(n,view)),4);
                float spec=pow(saturate(dot(n,normalize(sun.direction+view))),100)*.7;
                float3 color=lerp(_BaseColor.rgb,float3(.3,.51,.62),fres)+sun.color*spec;
                color+=float3(.006,.018,.02)*(a+b)*detail;
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
    }
}
