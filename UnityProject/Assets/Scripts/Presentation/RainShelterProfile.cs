using UnityEngine;
namespace VectorRush
{
    public sealed class RainShelterProfile : ScriptableObject
    {
        // Positive = distance into a roofed section; negative = distance outside it.
        public float[] signedRoofDistance;
        public float Distance(float progress)
        {
            if(signedRoofDistance==null||signedRoofDistance.Length==0)return -100;
            float x=Mathf.Repeat(progress,1)*signedRoofDistance.Length;int i=Mathf.FloorToInt(x);
            return Mathf.Lerp(signedRoofDistance[i],signedRoofDistance[(i+1)%signedRoofDistance.Length],x-i);
        }
        public static float WetnessAtDistance(float distance)=>Mathf.SmoothStep(0,1,Mathf.Clamp01(-distance/8f));
        public static float RainAudibilityAtDistance(float distance)=>distance<=0?1:Mathf.Exp(-distance/8f)*.12f;
        public static float FogExposureAtDistance(float distance)=>1-Mathf.SmoothStep(0,1,Mathf.Clamp01(distance/24f));
        public float Wetness(float progress)=>WetnessAtDistance(Distance(progress));
    }
}
