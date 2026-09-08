using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VectorRush
{
    public sealed class NightTrackLighting : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();
        public void Build(WorldBuilder world,TrackPath track)
        {
            var housing=world.MakeMaterial("Anodized light housings",new Color(.025f,.04f,.063f),.64f,.7f);
            var cool=world.MakeMaterial("Cool white linear lamps",new Color(.35f,.65f,.78f),.3f,0,new Color(2.4f,5.1f,6.4f));
            var amber=world.MakeMaterial("Amber service lamps",new Color(.7f,.3f,.035f),.3f,0,new Color(6.5f,2.3f,.3f));
            var paint=world.MakeMaterial("Road technical stencils",new Color(.45f,.55f,.58f),.2f);
            var pools=new List<CombineInstance>();var fixtures=new List<CombineInstance>();var warmFixtures=new List<CombineInstance>();
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var cube=primitive.GetComponent<MeshFilter>().sharedMesh;Destroy(primitive);
            for(int i=0;i<58;i++){
                var f=track.Evaluate(i/58f);float side=i%2==0?-1:1;var q=Quaternion.LookRotation(f.Forward,f.Up);
                bool warm=i%7==3;Vector3 p=f.Position+f.Right*side*14;
                Add(p+f.Up*6.4f,new Vector3(.22f,13,.36f),q,pools,cube);
                Add(p+f.Up*12.8f-f.Right*side*3.8f,new Vector3(8,.27f,.65f),q,pools,cube);
                Add(p+f.Up*12.6f-f.Right*side*4.5f,new Vector3(4.8f,.06f,.42f),q,warm?warmFixtures:fixtures,cube);
                var go=new GameObject(warm?"Amber road pool":"Cool road pool");go.transform.SetParent(transform,false);
                go.transform.position=p+f.Up*12.4f-f.Right*side*4;
                go.transform.rotation=Quaternion.LookRotation((f.Position+f.Forward*6-go.transform.position).normalized,f.Forward);
                var light=go.AddComponent<Light>();light.type=LightType.Spot;light.color=warm?new Color(1,.48f,.12f):new Color(.46f,.74f,1);
                light.intensity=warm?135:115;light.range=41;light.spotAngle=106;light.innerSpotAngle=67;light.shadows=LightShadows.None;
                // Repeated embedded edge reflectors and exposed deck engineering establish close-range scale.
                for(int s=-1;s<=1;s+=2){
                    world.Box("Barrier service panel",f.Position+f.Right*s*11.59f+f.Up*.91f,new Vector3(.045f,.62f,2.3f),q,housing);
                    world.Box("Navigation dash",f.Position+f.Right*s*11.55f+f.Up*1.15f,new Vector3(.055f,.09f,1.2f),q,warm?amber:cool);
                    world.Box("Deck cable housing",f.Position+f.Right*s*12.35f-f.Up*.65f,new Vector3(.6f,1.2f,19),q,housing);
                }
                if(i%3==0){
                    var label=new GameObject("Circuit station");label.transform.SetParent(transform,false);label.transform.SetPositionAndRotation(f.Position-f.Right*8.3f+f.Up*.065f,q*Quaternion.Euler(90,0,0));
                    var text=label.AddComponent<TextMesh>();text.text=(i*31).ToString("D4")+"  //";text.fontSize=48;text.characterSize=.085f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.22f,.37f,.43f);
                }
            }
            Combine("Track lighting steelwork",pools,housing);Combine("Cool linear road lamps",fixtures,cool);Combine("Amber linear road lamps",warmFixtures,amber);
            // Two short, open lighting galleries create rhythm and parallax while keeping the view clear.
            for(int section=0;section<2;section++)for(int i=0;i<7;i++){
                var f=track.Evaluate((section==0?.39f:.86f)+i*.007f);var q=Quaternion.LookRotation(f.Forward,f.Up);
                for(int side=-1;side<=1;side+=2){
                    world.Box("Gallery support",f.Position+f.Right*side*14.4f+f.Up*7.5f,new Vector3(.65f,15,.7f),q,housing);
                    world.Box("Gallery inner luminous edge",f.Position+f.Right*side*14f+f.Up*7.3f,new Vector3(.09f,13,.18f),q,section==0?cool:amber);
                }
                world.Box("Gallery crown beam",f.Position+f.Up*15,new Vector3(29,.7f,1.1f),q,housing);
                world.Box("Gallery ceiling strip",f.Position+f.Up*14.6f,new Vector3(24,.08f,.22f),q,section==0?cool:amber);
            }
        }
        static void Add(Vector3 p,Vector3 size,Quaternion q,List<CombineInstance> list,Mesh mesh){list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,size)});}
        void Combine(string name,List<CombineInstance> instances,Material material){var mesh=new Mesh{name=name};mesh.CombineMeshes(instances.ToArray());meshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;}
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
    }
}
