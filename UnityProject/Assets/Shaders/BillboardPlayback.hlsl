#ifndef VECTOR_BILLBOARD_PLAYBACK
#define VECTOR_BILLBOARD_PLAYBACK
TEXTURE2D(_Frames0); SAMPLER(sampler_Frames0);
TEXTURE2D(_Frames1); SAMPLER(sampler_Frames1);
TEXTURE2D(_Frames2); SAMPLER(sampler_Frames2);
TEXTURE2D(_Frames3); SAMPLER(sampler_Frames3);
TEXTURE2D(_Ticker); SAMPLER(sampler_Ticker);
TEXTURE2D(_Titles); SAMPLER(sampler_Titles);
float _BillboardTime;
float4 _BillboardDurations;
half3 CampaignFrame(float2 uv, int campaign, float frame,float mip)
{
    // Each 8x8 atlas contains 64 actual video frames. Insets prevent tile bleeding.
    float2 inset=(exp2(mip)*.75+1)/float2(384,576);
    uv=clamp(uv,inset,1-inset);
    float2 tile=float2(fmod(frame,8),7-floor(frame/8));
    float2 p=(tile+uv)/8;
    if(campaign==0)return SAMPLE_TEXTURE2D_LOD(_Frames0,sampler_Frames0,p,mip).rgb;
    if(campaign==1)return SAMPLE_TEXTURE2D_LOD(_Frames1,sampler_Frames1,p,mip).rgb;
    if(campaign==2)return SAMPLE_TEXTURE2D_LOD(_Frames2,sampler_Frames2,p,mip).rgb;
    return SAMPLE_TEXTURE2D_LOD(_Frames3,sampler_Frames3,p,mip).rgb;
}
half3 BillboardPlayback(float2 uv,int campaign,float phase,float aspect,float style,float2 crop,float minimumMip)
{
    float t=_BillboardTime+phase;
    if(style>.5 && style<1.5){
        float2 q=float2(frac(uv.x*.8+t*.018),(campaign+lerp(.08,.92,uv.y))/4);
        return SAMPLE_TEXTURE2D(_Ticker,sampler_Ticker,q).rgb;
    }
    uv.x=crop.x+uv.x*crop.y;
    float2 dx=ddx(uv)*float2(384,576),dy=ddy(uv)*float2(384,576);
    float mip=clamp(max(minimumMip,.5*log2(max(1,max(dot(dx,dx),dot(dy,dy))))),0,5);
    float cycle=frac(t/max(.1,_BillboardDurations[campaign]));
    float frame=campaign==2 ? (1-abs(cycle*2-1))*63 : cycle*64;
    float2 p=uv;if(style<.5)p.y=(p.y-.11)/.89;
    half3 color;
    if(style<.5 && aspect>1.05 && uv.x>.52)
    {
        float2 card=float2((uv.x-.52)/.48,saturate(p.y));
        color=SAMPLE_TEXTURE2D(_Titles,sampler_Titles,float2(card.x,(campaign+card.y)/4)).rgb;
        // A moving spectrum accompanies the stationary brand headline.
        float wave=.18+.10*sin(card.x*19+t*1.8)+.05*sin(card.x*47-t*2.6);
        color+=half3(.18,.38,.42)*smoothstep(.015,0,abs(card.y-wave));
    }
    else
    {
        if(style<.5 && aspect>1.05)p.x=uv.x/.52;
        color=lerp(CampaignFrame(p,campaign,floor(frame),mip),CampaignFrame(p,campaign,fmod(floor(frame)+1,64),mip),frac(frame));
    }
    if(style<.5 && uv.y<.105)
    {
        float2 q=float2(frac(uv.x*aspect*.55+t*.018),(campaign+uv.y/.105)/4);
        color=SAMPLE_TEXTURE2D(_Ticker,sampler_Ticker,q).rgb;
    }
    return color;
}
#endif
