Shader "VectorRush/Night Sky"
{
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; };struct V{float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;};
            V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.direction=i.positionOS.xyz;return o;}
            half4 frag(V i):SV_Target{
                float3 d=normalize(i.direction);float horizon=pow(1-saturate(d.y),5);
                float3 color=lerp(float3(.0015,.0028,.009),float3(.039,.053,.084),horizon);
                float haze=sin(d.x*13+d.z*7)*sin(d.x*5-d.z*11)*.0015*horizon;
                color+=haze;
                float moon=dot(d,normalize(float3(-.35,.5,.75)));
                color+=float3(.15,.22,.34)*pow(saturate(moon),900);
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}
