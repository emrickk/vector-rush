Shader "VectorRush/Ion Trail"
{
    Properties
    {
        [HDR] _Tint("Ion color",Color)=(0.04,0.9,1.8,1)
        _Intensity("Intensity",Float)=1
        _Radial("Radial core",Float)=0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR;half fog:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;float _Intensity;float _Radial;
            CBUFFER_END
            V vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;o.color=v.color;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            half4 frag(V i):SV_Target
            {
                float edge=pow(saturate(1-abs(i.uv.y*2-1)),1.8);
                float2 p=(i.uv-.5)*2;float radius=length(p);
                float core=exp(-dot(p,p)*5)*smoothstep(1,.35,radius);
                float shape=lerp(edge,core,saturate(_Radial));
                half3 color=_Tint.rgb*i.color.rgb*i.color.a*_Tint.a*_Intensity*shape;
                return half4(MixFogColor(color,half3(0,0,0),i.fog),0);
            }
            ENDHLSL
        }
    }
}
