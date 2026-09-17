Shader "VectorRush/Blue Wake Particle"
{
    SubShader
    {
        Tags { "Queue"="Transparent+12" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            struct A{float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;};
            struct V{float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;half fog:TEXCOORD1;};
            V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            float hash21(float2 p){p=frac(p*float2(123.34,345.45));p+=dot(p,p+34.345);return frac(p.x*p.y);}
            float noise21(float2 p)
            {
                float2 c=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(hash21(c),hash21(c+float2(1,0)),f.x),lerp(hash21(c+float2(0,1)),hash21(c+1),f.x),f.y);
            }
            half4 frag(V i):SV_Target
            {
                float2 p=(i.uv-.5)*2;
                float r=dot(p,p);
                float envelope=exp(-r*3.2)*(1-smoothstep(.50,1,r));
                float detail=.65+.35*noise21(p*3.1+float2(7.2,4.8));
                half3 radiance=i.color.rgb*i.color.a*envelope*detail*3.0;
                return half4(MixFogColor(radiance,half3(0,0,0),i.fog),0);
            }
            ENDHLSL
        }
    }
}
