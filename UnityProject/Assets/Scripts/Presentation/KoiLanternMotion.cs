using UnityEngine;
namespace VectorRush
{
    // Purely visual mechanical fins; no racing state or physics interaction.
    public sealed class KoiLanternMotion : MonoBehaviour
    {
        public Transform[] fins;
        Quaternion[] rest;
        void Awake(){rest=new Quaternion[fins.Length];for(int i=0;i<fins.Length;i++)rest[i]=fins[i].localRotation;}
        void LateUpdate(){for(int i=0;i<fins.Length;i++)if(fins[i])fins[i].localRotation=rest[i]*Quaternion.AngleAxis(Mathf.Sin(Time.time*.65f+i*1.3f)*(i==3?4:2.5f),Vector3.up);}
        void OnDisable(){if(rest==null)return;for(int i=0;i<fins.Length;i++)if(fins[i])fins[i].localRotation=rest[i];}
    }
}
