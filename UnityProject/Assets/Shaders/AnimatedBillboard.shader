Shader "VectorRush/Animated Billboard"
{
    Properties
    {
        _Frames0("Aurora motion",2D)="black"{}
        _Frames1("Volt motion",2D)="black"{}
        _Frames2("Echo motion",2D)="black"{}
        _Frames3("Orbit motion",2D)="black"{}
        _Ticker("Scrolling headlines",2D)="black"{}
        _Titles("Landscape typography",2D)="black"{}
        _Campaign("Campaign",Float)=0
        _Phase("Independent start time",Float)=0
        _Aspect("Display aspect",Float)=.66
        _Style("Composition style",Float)=0
        _CropStart("Segment start",Float)=0
        _CropWidth("Segment width",Float)=1
        _Intensity("Display luminance",Float)=1.65
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Cull Back
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "BillboardPlayback.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Campaign,_Phase,_Aspect,_Intensity,_Style,_CropStart,_CropWidth;
            CBUFFER_END
            struct A{float4 vertex:POSITION;float2 uv:TEXCOORD0;};
            struct V{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float fog:TEXCOORD1;};
            V Vert(A v){V o;o.vertex=TransformObjectToHClip(v.vertex.xyz);o.uv=v.uv;o.fog=ComputeFogFactor(o.vertex.z);return o;}
            half4 Frag(V i):SV_Target{return half4(MixFog(BillboardPlayback(i.uv,(int)_Campaign,_Phase,_Aspect,_Style,float2(_CropStart,_CropWidth),0)*_Intensity,i.fog),1);}
            ENDHLSL
        }
    }
}
