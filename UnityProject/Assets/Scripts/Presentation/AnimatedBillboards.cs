using UnityEngine;
namespace VectorRush
{
    // Shared GPU video-atlas clock: no decoder, per-frame texture upload or gameplay RNG.
    public sealed class AnimatedBillboards : MonoBehaviour
    {
        public Transform[] displays;
        public Transform[] kineticObjects;
        Vector3[] origins;
        Quaternion[] rotations;
        static readonly int Clock=Shader.PropertyToID("_BillboardTime");
        public static readonly Vector4 PlaybackSeconds=new Vector4(7.5f,4.5f,12f,4.25f);
        public static float FrameAtTime(float seconds,float phase,int campaign=0)
        {
            float fraction=Mathf.Repeat((seconds+phase)/PlaybackSeconds[campaign],1);
            return campaign==2?(1-Mathf.Abs(fraction*2-1))*63:fraction*64;
        }
        void Awake()
        {
            Shader.SetGlobalVector("_BillboardDurations",PlaybackSeconds);
            origins=new Vector3[kineticObjects.Length];rotations=new Quaternion[kineticObjects.Length];
            for(int i=0;i<origins.Length;i++){origins[i]=kineticObjects[i].localPosition;rotations[i]=kineticObjects[i].localRotation;}
        }
        void Update()
        {
            float t=Time.time;Shader.SetGlobalFloat(Clock,t);
            for(int i=0;i<kineticObjects.Length;i++){
                kineticObjects[i].localRotation=rotations[i]*Quaternion.Euler(12*Mathf.Sin(t*.65f+i),t*(15+i*6),9*Mathf.Sin(t*.47f+i));
                kineticObjects[i].localPosition=origins[i]+Vector3.up*(Mathf.Sin(t*1.1f+i)*.35f);
            }
        }
    }
}
