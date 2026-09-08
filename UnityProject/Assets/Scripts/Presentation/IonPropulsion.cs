using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    /// <summary>Dedicated additive exhaust, separated from opaque craft and track materials.</summary>
    public sealed class IonPropulsion : MonoBehaviour
    {
        HoverVehicle vehicle;
        Material wakeMaterial,coreMaterial;
        readonly TrailRenderer[] wakes=new TrailRenderer[2];
        readonly Renderer[] cores=new Renderer[2];
        readonly Light[] lights=new Light[2];
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();

        public void Initialize(HoverVehicle craft)
        {
            vehicle=craft;
            var shader=Shader.Find("VectorRush/Ion Trail");
            if(!shader){Debug.LogError("Ion Trail shader missing from build");enabled=false;return;}
            wakeMaterial=new Material(shader){name="Transparent ion wake"};
            coreMaterial=new Material(shader){name="Radiant thrust core"};
            coreMaterial.SetFloat("_Radial",1);
            for(int i=0;i<2;i++){
                var anchor=new GameObject("Port "+(i==0?"L":"R")+" ion nozzle");
                anchor.transform.SetParent(craft.VisualRoot?craft.VisualRoot:craft.transform,false);
                anchor.transform.localPosition=new Vector3(i==0?-1.68f:1.68f,-.035f,-3.43f);
                var trail=anchor.AddComponent<TrailRenderer>();wakes[i]=trail;
                trail.sharedMaterial=wakeMaterial;trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;
                trail.textureMode=LineTextureMode.Stretch;trail.minVertexDistance=.2f;trail.numCapVertices=2;trail.numCornerVertices=2;
                trail.widthCurve=new AnimationCurve(new Keyframe(0,.25f),new Keyframe(.2f,.13f),new Keyframe(1,0));
                var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.1f,.55f,1),1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(.18f,.3f),new GradientAlphaKey(0,1)});trail.colorGradient=gradient;
                var core=GameObject.CreatePrimitive(PrimitiveType.Quad);core.name="Recessed ion radiance";core.transform.SetParent(anchor.transform,false);core.transform.localPosition=Vector3.forward*.57f;core.transform.localScale=Vector3.one*.68f;Destroy(core.GetComponent<Collider>());
                cores[i]=core.GetComponent<Renderer>();cores[i].sharedMaterial=coreMaterial;cores[i].shadowCastingMode=ShadowCastingMode.Off;cores[i].receiveShadows=false;
                if(craft.IsPlayer){lights[i]=anchor.AddComponent<Light>();lights[i].type=LightType.Point;lights[i].color=new Color(.03f,.8f,1);lights[i].range=4;lights[i].shadows=LightShadows.None;}
            }
        }
        void LateUpdate()
        {
            if(!vehicle||!wakeMaterial)return;
            float speed=Mathf.Clamp01(vehicle.SpeedKph/340f);bool boost=vehicle.IsBoosting;
            float intensity=(boost?5.5f:1.4f+speed*1.4f)*(1+.025f*Mathf.Sin(Time.time*47));
            properties.SetFloat("_Intensity",intensity);
            for(int i=0;i<2;i++){
                wakes[i].emitting=vehicle.SpeedKph>18;wakes[i].time=boost?.18f:Mathf.Lerp(.035f,.08f,speed);wakes[i].widthMultiplier=boost?1.7f:1;
                cores[i].SetPropertyBlock(properties);
                if(lights[i])lights[i].intensity=boost?2.8f:.65f+speed*.6f;
            }
        }
        void OnDestroy(){if(wakeMaterial)Destroy(wakeMaterial);if(coreMaterial)Destroy(coreMaterial);}
    }
}
