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
        public static float FrameAtTime(float seconds,float phase)=>Mathf.Repeat((seconds+phase)/5f,1)*64;
        void Awake()
        {
            origins=new Vector3[kineticObjects.Length];rotations=new Quaternion[kineticObjects.Length];
            for(int i=0;i<origins.Length;i++){origins[i]=kineticObjects[i].localPosition;rotations[i]=kineticObjects[i].localRotation;}
        }
        void Update()
        {
            float t=Time.time;Shader.SetGlobalFloat(Clock,t);
            for(int i=0;i<kineticObjects.Length;i++){
                kineticObjects[i].localRotation=rotations[i]*Quaternion.Euler(12*Mathf.Sin(t*.65f+i),t*(22+i*6),9*Mathf.Sin(t*.47f+i));
                kineticObjects[i].localPosition=origins[i]+Vector3.up*(Mathf.Sin(t*1.1f+i)*.35f);
            }
        }
    }
}
