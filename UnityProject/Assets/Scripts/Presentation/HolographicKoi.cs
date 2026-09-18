using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace VectorRush
{
    // The fish swims in world space; the GPU adds the coherent body and fin wave.
    public sealed class HolographicKoi : MonoBehaviour
    {
        public Transform fish;
        public Material reflectionMaterial;
        public Vector3 swimAcross, swimAlong;
        public Vector3 anchor;
        public Quaternion heading;
        public float period=18;
        public Light[] movingLights;
        Camera capture;
        RenderTexture texture;
        Material instance;
        public void ApplySwimPose(float seconds)
        {
            float phase=seconds*Mathf.PI*2/period;
            fish.position=anchor+swimAcross*(22*Mathf.Sin(phase))+swimAlong*(14*Mathf.Sin(phase*2))+Vector3.up*(1.5f*Mathf.Sin(phase+.7f));
            fish.rotation=heading*Quaternion.Euler(.8f*Mathf.Sin(phase*2),14*Mathf.Cos(phase),.8f*Mathf.Sin(phase));
            if(movingLights!=null)for(int i=0;i<movingLights.Length;i++)if(movingLights[i])movingLights[i].transform.position=fish.position+Vector3.down*(i==0?15:22);
        }
        void Start()
        {
            texture=new RenderTexture(768,384,16,RenderTextureFormat.ARGBHalf){name="Live swimming koi reflection"};texture.Create();
            var go=new GameObject("Koi reflection capture");go.transform.SetParent(transform,false);capture=go.AddComponent<Camera>();capture.orthographic=true;capture.orthographicSize=30*fish.localScale.x;capture.aspect=2;capture.nearClipPlane=.5f;capture.farClipPlane=500;capture.backgroundColor=Color.black;capture.clearFlags=CameraClearFlags.SolidColor;capture.cullingMask=1<<29;capture.allowHDR=true;capture.targetTexture=texture;capture.depth=-10;
            var extra=capture.GetUniversalAdditionalCameraData();extra.renderPostProcessing=false;extra.renderShadows=false;
            instance=new Material(reflectionMaterial);foreach(var r in GetComponentsInChildren<MeshRenderer>())if(r.sharedMaterial==reflectionMaterial)r.sharedMaterial=instance;
            instance.SetTexture("_Fish",texture);UpdateCapture();
        }
        void LateUpdate(){ApplySwimPose(Time.time);if(capture)UpdateCapture();}
        void UpdateCapture()
        {
            var center=fish.TransformPoint(new Vector3(-9,0,0));capture.transform.SetPositionAndRotation(center+fish.forward*240,Quaternion.LookRotation(-fish.forward,fish.up));
            instance.SetVector("_Center",center);instance.SetVector("_Right",-fish.right);instance.SetVector("_Up",fish.up);instance.SetVector("_Normal",fish.forward);instance.SetVector("_Size",new Vector4(120*fish.localScale.x,60*fish.localScale.x,0,0));
        }
        void OnDestroy(){if(texture){texture.Release();Destroy(texture);}if(instance)Destroy(instance);}
    }
}
