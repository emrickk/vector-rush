Shader "VectorRush/NightWindows"
{
    Properties
    {
        _BaseColor("Unlit glass", Color) = (.016,.03,.05,1)
        [HDR] _WarmColor("Occupied warm rooms", Color) = (2.3,1.05,.3,1)
        [HDR] _CoolColor("Occupied cool rooms", Color) = (.3,.85,1.5,1)
        _Density("Occupied fraction", Range(0,1)) = .36
        _Seed("District seed", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "NightFacade"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _WarmColor, _CoolColor;
                float _Density, _Seed;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half fog:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o; VertexPositionInputs p=GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS; o.normalWS=TransformObjectToWorldNormal(v.normalOS); o.fog=ComputeFogFactor(p.positionCS.z); return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7))+_Seed*37.7)*43758.5453); }
            half4 Frag(Varyings i):SV_Target
            {
                half3 n=normalize(i.normalWS); float horizontal=abs(n.x)>abs(n.z)?i.positionWS.z:i.positionWS.x;
                float2 rooms=float2(horizontal/2.6,i.positionWS.y/3.4);
                float2 cell=floor(rooms), within=frac(rooms);
                float edge=min(min(within.x-.13,.87-within.x),min(within.y-.14,.83-within.y));
                float panel=smoothstep(0,max(fwidth(edge),.015),edge)*(1-smoothstep(.65,.9,abs(n.y)));
                float occupancy=step(1-_Density,Hash(cell));
                half3 room=lerp(_WarmColor.rgb,_CoolColor.rgb,step(.79,Hash(cell+53)));
                float curtain=lerp(.40,1.0,step(.37,within.x));
                Light light=GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                half3 ambient=max(SampleSH(n),half3(.045,.06,.09));
                half3 base=_BaseColor.rgb*(ambient+light.color*saturate(dot(n,light.direction))*light.shadowAttenuation);
                half fresnel=pow(1-saturate(dot(n,GetWorldSpaceNormalizeViewDir(i.positionWS))),4);
                half3 color=base+half3(.025,.045,.075)*fresnel+room*occupancy*panel*curtain;
                return half4(MixFog(color,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
