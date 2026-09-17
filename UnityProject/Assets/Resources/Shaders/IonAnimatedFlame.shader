Shader "VectorRush/Animated Blue Flame"
{
    Properties
    {
        _MainTex("Animated flame atlas",2D)="white"{}
        _UseAtlasColor("Use atlas RGB and alpha",Float)=0
        _Intensity("Radiance",Float)=1
        _ColorWhitening("Hot blue core lift",Range(0,1))=0
    }
    SubShader
    {
        Tags { "Queue"="Transparent+12" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float _UseAtlasColor;
            float _Intensity;
            float _ColorWhitening;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float eye:TEXCOORD1; half fog:TEXCOORD2; };
            V vert(A i)
            {
                V o; float3 world=TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(world); o.uv=i.uv; o.color=i.color;
                o.eye=-TransformWorldToView(world).z; o.fog=ComputeFogFactor(o.positionCS.z); return o;
            }
            half4 frag(V i):SV_Target
            {
                half4 atlas=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half density=lerp(atlas.r,atlas.a,_UseAtlasColor);
                half3 atlasColor=lerp(half3(1,1,1),atlas.rgb,_UseAtlasColor);
                atlasColor=lerp(atlasColor,half3(1,1,1),_ColorWhitening);
                float raw=SampleSceneDepth(GetNormalizedScreenSpaceUV(i.positionCS));
                float sceneEye=LinearEyeDepth(raw,_ZBufferParams);
                half fade=saturate((sceneEye-i.eye)/.65)*smoothstep(.7,2.5,i.eye);
                half alpha=density*i.color.a*fade;
                half3 radiance=atlasColor*i.color.rgb*alpha*2.7*_Intensity;
                return half4(MixFogColor(radiance,half3(0,0,0),i.fog),alpha*.35);
            }
            ENDHLSL
        }
    }
}
