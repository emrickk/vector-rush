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
            var panel=world.MakeMaterial("Gallery charcoal ceramic panels",new Color(.17f,.20f,.23f),.34f,.18f);
            var trim=world.MakeMaterial("Gallery folded aluminum returns",new Color(.095f,.12f,.145f),.48f,.55f);
            var cool=world.MakeMaterial("Cool white linear lamps",new Color(.35f,.65f,.78f),.3f,0,new Color(2.4f,5.1f,6.4f));
            var amber=world.MakeMaterial("Amber service lamps",new Color(.7f,.3f,.035f),.3f,0,new Color(6.5f,2.3f,.3f));
            var paint=world.MakeMaterial("Road technical stencils",new Color(.45f,.55f,.58f),.2f);
            var pools=new List<CombineInstance>();var fixtures=new List<CombineInstance>();var warmFixtures=new List<CombineInstance>();
            var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var cube=primitive.GetComponent<MeshFilter>().sharedMesh;Destroy(primitive);
            for(int i=0;i<58;i++){
                var f=track.Evaluate(i/58f);float side=i%2==0?-1:1;var q=Quaternion.LookRotation(f.Forward,f.Up);
                bool warm=i%7==3;Vector3 p=f.Position+f.Right*side*14;
                float progress=i/58f;
                bool insideGallery=(progress>=.39f&&progress<=.432f)||(progress>=.86f&&progress<=.902f);
                if(!insideGallery){
                Add(p+f.Up*6.4f,new Vector3(.22f,13,.36f),q,pools,cube);
                Add(p+f.Up*12.8f-f.Right*side*3.8f,new Vector3(8,.27f,.65f),q,pools,cube);
                Add(p+f.Up*12.6f-f.Right*side*4.5f,new Vector3(4.8f,.06f,.42f),q,warm?warmFixtures:fixtures,cube);
                var go=new GameObject(warm?"Amber road pool":"Cool road pool");go.transform.SetParent(transform,false);
                go.transform.position=p+f.Up*12.4f-f.Right*side*4;
                go.transform.rotation=Quaternion.LookRotation((f.Position+f.Forward*6-go.transform.position).normalized,f.Forward);
                var light=go.AddComponent<Light>();light.type=LightType.Spot;light.color=warm?new Color(1,.48f,.12f):new Color(.68f,.86f,.91f);
                light.intensity=warm?540:460;light.range=41;light.spotAngle=98;light.innerSpotAngle=52;light.shadows=LightShadows.None;
                if(OpeningFinishPreview.SurfaceEnabled&&i>=7&&i<=18)OpeningFixture(light,f,side,i);
                }
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
            // Native A/B/A isolates the pole-and-arm moon shadows as the sharp exterior road bands.
            // Keep fixtures visible and lit while letting the satin deck's broad light pools read cleanly.
            Combine("Track lighting steelwork",pools,housing,ShadowCastingMode.Off);
            Combine("Cool linear road lamps",fixtures,cool,ShadowCastingMode.Off);
            Combine("Amber linear road lamps",warmFixtures,amber,ShadowCastingMode.Off);
            var structure=world.MakeMaterial("Gallery cast charcoal structure",new Color(.16f,.18f,.20f),.29f,.12f);
            var ceiling=world.MakeMaterial("Gallery matte acoustic ceiling",new Color(.22f,.235f,.25f),.18f,.04f);
            var warmPanel=world.MakeMaterial("Warm gallery ceramic panels",new Color(.235f,.21f,.18f),.3f,.08f);
            var warmLight=world.MakeMaterial("Gallery warm white diffuser",new Color(.9f,.65f,.36f),.25f,0,new Color(4.3f,2.55f,1.08f));
            var coolLight=world.MakeMaterial("Gallery cool white diffuser",new Color(.56f,.7f,.79f),.25f,0,new Color(2.45f,3.7f,4.55f));
            BuildGallery(track,0,.39f,cube,structure,ceiling,panel,trim,housing,coolLight,paint);
            if(OpeningFinishPreview.SurfaceEnabled){
                // Gallery meshes do not share the authored landmark UV contract. Distinguish broad
                // construction through material response; do not stamp metric normal maps onto them.
                var studyStructure=world.MakeMaterial("Warm study / dark structural ribs",new Color(.065f,.075f,.087f),.27f,.3f);
                // The swept backing approaches the flat cassette faces on this curved gallery.
                // Keep its tone close enough that existing intersections do not become dark facets.
                var studyShell=world.MakeMaterial("Warm study / recessed ceiling and backing",new Color(.24f,.232f,.212f),.28f,.08f);
                var studyPanel=world.MakeMaterial("Warm study / neutral ceramic faces",new Color(.265f,.255f,.228f),.34f,.12f);
                var studyTrim=world.MakeMaterial("Warm study / satin folded returns",new Color(.20f,.215f,.23f),.43f,.65f);
                BuildGallery(track,1,.86f,cube,studyStructure,studyShell,studyPanel,studyTrim,housing,warmLight,paint);
            }else BuildGallery(track,1,.86f,cube,structure,ceiling,warmPanel,trim,housing,warmLight,paint);
        }

        static void OpeningFixture(Light light,TrackFrame frame,float side,int index)
        {
            // Reuse the twelve visible sources at .121–.310. Lit approach (7–9), a quieter
            // transition (10–12), lit bend (13–15), then descent (16–18). No extra light count.
            float[] intensity={520,780,860,400,190,330,820,900,530,260,650,640};
            float[] forward={7,9,10,7,5,7,9,11,8,6,8,10};
            int slot=index-7;
            // Source sits immediately below the actual 4.8 m diffuser, at its lateral center.
            light.transform.position=frame.Position+frame.Right*side*9.5f+frame.Up*12.48f;
            var target=frame.Position+frame.Forward*forward[slot]-frame.Right*side*1.5f;
            light.transform.rotation=Quaternion.LookRotation((target-light.transform.position).normalized,frame.Forward);
            light.intensity=intensity[slot];light.range=43;
            // Keep the same intensity budget. Less overlapping, oblique footprints leave
            // readable dark intervals and put the inner cone closer to each physical source.
            light.spotAngle=(index==11||index==16)?68:72;light.innerSpotAngle=(index==11||index==16)?32:38;
            light.gameObject.name+=" / opening authored coverage";
        }

        void BuildGallery(TrackPath track,int section,float start,Mesh cube,Material structure,Material ceiling,
            Material ceramic,Material trim,Material housing,Material diffuser,Material paint)
        {
            const float step=.007f;
            var ribs=new List<CombineInstance>();var shells=new List<CombineInstance>();
            var panels=new List<CombineInstance>();var metal=new List<CombineInstance>();
            var dark=new List<CombineInstance>();var lamps=new List<CombineInstance>();var labels=new List<CombineInstance>();
            var ribMesh=CreateRib();meshes.Add(ribMesh);
            bool warm=section==1;
            bool surfaceStudy=warm&&OpeningFinishPreview.SurfaceEnabled;
            for(int i=0;i<7;i++){
                var f=track.Evaluate(start+i*step);var q=Quaternion.LookRotation(f.Forward,f.Up);
                bool entry=i==0,exit=i==6;
                float depth=entry?1.65f:exit?1.1f:.68f;
                Add(f.Position,new Vector3(1,1,depth),q,ribs,ribMesh);
                for(int side=-1;side<=1;side+=2){
                    // The structural feet remain behind the existing barriers and outside the racing envelope.
                    Add(f.Position+f.Right*side*14.4f+f.Up*.65f,new Vector3(1.75f,1.3f,depth+.65f),q,ribs,cube);
                    Add(f.Position+f.Right*side*14.4f+f.Up*1.42f,new Vector3(1.4f,.17f,depth+.3f),q,metal,cube);
                    if(entry||exit){
                        // Short portal identification lights live in a recess, rather than tracing every upright.
                        Add(f.Position+f.Right*side*13.78f+f.Up*8.7f,new Vector3(.18f,2.2f,depth+.18f),q,dark,cube);
                        Add(f.Position+f.Right*side*13.67f+f.Up*8.7f,new Vector3(.04f,1.55f,depth+.2f),q,lamps,cube);
                    }
                }
                if(entry){
                    // A second frame and a deep brow give the entrance a visible threshold.
                    Add(f.Position-f.Forward*2.2f,new Vector3(1.035f,1.025f,.52f),q,ribs,ribMesh);
                    Add(f.Position-f.Forward*.8f+f.Up*16.1f,new Vector3(23.6f,.8f,3.6f),q,ribs,cube);
                    Add(f.Position-f.Forward*2.65f+f.Up*15.48f,new Vector3(19.4f,.18f,.22f),q,dark,cube);
                    Add(f.Position-f.Forward*2.78f+f.Up*15.47f,new Vector3(17.8f,.075f,.055f),q,lamps,cube);
                }
                if(exit){
                    // The last span loses the heavy fascia so the cool skyline opens through a slim frame.
                    Add(f.Position+f.Forward*.65f+f.Up*15.48f,new Vector3(20.6f,.14f,.65f),q,metal,cube);
                }
                if(i==6)continue;
                var next=track.Evaluate(start+(i+1)*step);
                var mid=track.Evaluate(start+(i+.5f)*step);var mq=Quaternion.LookRotation(mid.Forward,mid.Up);
                float length=Vector3.Distance(f.Position,next.Position);
                // Swept shells follow the bank and curve; inset cassette seams remain intentional.
                AddSpan(f,next,new Vector2(-10.5f,15.48f),new Vector2(10.5f,15.48f),.44f,shells);
                for(int row=0;row<3;row++){
                    float x=(row-1)*6.45f;
                    Add(mid.Position+mid.Right*x+mid.Up*15.205f,new Vector3(6.16f,.12f,length-1.55f),mq,panels,cube);
                    Add(mid.Position+mid.Right*(x+3.14f)+mid.Up*15.09f,new Vector3(.12f,.18f,length-1.3f),mq,metal,cube);
                }
                for(int side=-1;side<=1;side+=2){
                    AddSpan(f,next,new Vector2(side*14.68f,1.45f),new Vector2(side*14.68f,11.25f),.38f,shells);
                    AddSpan(f,next,new Vector2(side*14.68f,11.25f),new Vector2(side*10.5f,15.48f),.3f,shells);
                    // Two large panels per bay have a dark backing, folded joints and a lower maintenance channel.
                    for(int k=0;k<2;k++){
                        var pf=track.Evaluate(start+(i+.25f+k*.5f)*step);var pq=Quaternion.LookRotation(pf.Forward,pf.Up);
                        Add(pf.Position+pf.Right*side*14.43f+pf.Up*7.05f,new Vector3(.13f,7.35f,length*.5f-.32f),pq,panels,cube);
                        Add(pf.Position+pf.Right*side*14.32f+pf.Up*10.9f,new Vector3(.25f,.2f,length*.5f-.22f),pq,metal,cube);
                        Add(pf.Position+pf.Right*side*14.3f+pf.Up*3.18f,new Vector3(.22f,.26f,length*.5f-.2f),pq,metal,cube);
                    }
                    Add(mid.Position+mid.Right*side*14.28f+mid.Up*2.65f,new Vector3(.25f,.56f,length-.9f),mq,dark,cube);
                    Add(mid.Position+mid.Right*side*14.12f+mid.Up*3.58f,new Vector3(.24f,.27f,length-1.2f),mq,dark,cube);
                    Add(mid.Position+mid.Right*side*13.98f+mid.Up*3.59f,new Vector3(.035f,.075f,length-2.3f),mq,lamps,cube);
                    if(i%2==0)Add(mid.Position+mid.Right*side*14.10f+mid.Up*2.65f,new Vector3(.03f,.27f,1.45f),mq,labels,cube);
                    // Concealed upper fixtures cast real light onto both ceiling and wall surfaces.
                    // These replace the previous two downward wall spots, retaining three lights per bay.
                    Add(mid.Position+mid.Right*side*12.88f+mid.Up*10.53f,new Vector3(2.95f,.14f,.24f),mq,metal,cube);
                    Add(mid.Position+mid.Right*side*11.42f+mid.Up*10.66f,new Vector3(.7f,.22f,3.3f),mq,dark,cube);
                    Add(mid.Position+mid.Right*side*11.35f+mid.Up*10.79f,new Vector3(.34f,.04f,2.65f),mq,lamps,cube);
                    var wash=new GameObject(warm?"Warm gallery concealed surface wash":"Cool gallery concealed surface wash");
                    wash.transform.SetParent(transform,false);
                    // Pull the cool wash away from the wall and lower its peak so the panel reads as a surface.
                    wash.transform.position=mid.Position+mid.Right*side*(warm?11.32f:10.6f)+mid.Up*10.97f;
                    var fill=wash.AddComponent<Light>();fill.type=LightType.Point;
                    fill.color=warm?new Color(1,.72f,.46f):new Color(.76f,.85f,1);
                    fill.intensity=(warm?88:52)*(i==5?.62f:i%3==1?1f:.86f)*(side<0?.88f:1f);
                    fill.range=warm?20:22;fill.shadows=LightShadows.None;
                    if(surfaceStudy){
                        // Retain omnidirectional concealed practicals so shoulders and ceilings still
                        // receive light; match the emitter origin and reduce the uniform amber wash.
                        wash.transform.position=mid.Position+mid.Right*side*11.35f+mid.Up*10.86f;
                        fill.color=new Color(1,.76f,.54f);
                        fill.intensity=(i==5?52:i==2?65:i==1?90:i==3?88:76)*(side<0?.82f:1f);
                        fill.range=18;
                    }
                    if((i==2&&side==1)||(i==4&&side==-1)){
                        // Designated service bays interrupt the blank repeated wall spans.
                        // Attach the hatch to one half-panel, not the joint between two angled panels.
                        var sf=track.Evaluate(start+(i+.25f)*step);var sq=Quaternion.LookRotation(sf.Forward,sf.Up);
                        var service=sf.Position+sf.Right*side*14.27f+sf.Up*6.25f;
                        Add(service,new Vector3(.1f,3.6f,3.5f),sq,dark,cube);
                        for(int vent=0;vent<5;vent++)
                            Add(service+sf.Up*(-1.15f+vent*.48f)-sf.Right*side*.08f,new Vector3(.12f,.12f,3.12f),sq,metal,cube);
                        Add(service+sf.Forward*1.82f,new Vector3(.18f,3.85f,.13f),sq,metal,cube);
                        Add(service-sf.Forward*1.82f,new Vector3(.18f,3.85f,.13f),sq,metal,cube);
                    }
                }
                // One transverse diffuser is recessed into a substantial tray at the rear of each cassette.
                var fixture=track.Evaluate(start+(i+.18f)*step);var fq=Quaternion.LookRotation(fixture.Forward,fixture.Up);
                Add(fixture.Position+fixture.Up*14.99f,new Vector3(19.5f,.3f,.78f),fq,dark,cube);
                Add(fixture.Position+fixture.Up*14.80f,new Vector3(17.7f,.065f,.24f),fq,lamps,cube);
                Add(fixture.Position+fixture.Up*14.94f+fixture.Forward*.4f,new Vector3(19.8f,.25f,.09f),fq,metal,cube);
                var road=new GameObject(warm?"Warm gallery road pool":"Cool gallery road pool");road.transform.SetParent(transform,false);
                road.transform.position=fixture.Position+fixture.Up*14.68f;
                road.transform.rotation=Quaternion.LookRotation(-fixture.Up+fixture.Forward*.22f,fixture.Forward);
                var light=road.AddComponent<Light>();light.type=LightType.Spot;
                light.color=warm?new Color(1,.64f,.36f):new Color(.7f,.85f,1);
                light.intensity=warm?285:245;light.range=30;light.spotAngle=100;light.innerSpotAngle=52;light.shadows=LightShadows.None;
                if(surfaceStudy){
                    // Alternating existing cassette sources create dark intervals and broad diagonal
                    // coverage. This is direct specular response, not an image of reflected fixtures.
                    float[] intensity={360,440,260,420,360,240};
                    var target=fixture.Position+fixture.Forward*(i%2==0?6f:4f)+fixture.Right*(i%2==0?3.5f:-3.5f);
                    road.transform.rotation=Quaternion.LookRotation((target-road.transform.position).normalized,fixture.Forward);
                    light.color=new Color(1,.78f,.55f);light.intensity=intensity[i];
                    light.spotAngle=74;light.innerSpotAngle=36;light.range=31;
                }
            }
            string prefix=warm?"Warm gallery ":"Cool gallery ";
            Combine(prefix+"primary portal structure",ribs,structure);Combine(prefix+"continuous shell",shells,ceiling);
            Combine(prefix+"recessed ceramic cassettes",panels,ceramic);Combine(prefix+"folded service returns",metal,trim);
            Combine(prefix+"light and maintenance housings",dark,housing);Combine(prefix+"integrated diffusers",lamps,diffuser);
            Combine(prefix+"maintenance markings",labels,paint);
        }

        // A continuous polygonal portal keeps broad, flat structural faces at the chamfered shoulders.
        static Mesh CreateRib()
        {
            var inner=new[]{new Vector2(-13.85f,0),new Vector2(-13.85f,10.6f),new Vector2(-10.1f,14.6f),
                new Vector2(10.1f,14.6f),new Vector2(13.85f,10.6f),new Vector2(13.85f,0)};
            var outer=new[]{new Vector2(-14.95f,0),new Vector2(-14.95f,11.05f),new Vector2(-10.58f,15.8f),
                new Vector2(10.58f,15.8f),new Vector2(14.95f,11.05f),new Vector2(14.95f,0)};
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<inner.Length-1;i++){
                var a=new Vector3(inner[i].x,inner[i].y,-.5f);var b=new Vector3(inner[i+1].x,inner[i+1].y,-.5f);
                var c=new Vector3(outer[i+1].x,outer[i+1].y,-.5f);var d=new Vector3(outer[i].x,outer[i].y,-.5f);
                AddPrism(vertices,triangles,a,b,c,d,Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward);
            }
            return MeshFrom("Chamfered gallery portal",vertices,triangles);
        }

        void AddSpan(TrackFrame a,TrackFrame b,Vector2 from,Vector2 to,float thickness,List<CombineInstance> list)
        {
            Vector2 normal=new Vector2(-(to.y-from.y),to.x-from.x).normalized*thickness*.5f;
            Vector2 p0=from-normal,p1=to-normal,p2=to+normal,p3=from+normal;
            Vector3 A(Vector2 p)=>a.Position+a.Right*p.x+a.Up*p.y;
            Vector3 B(Vector2 p)=>b.Position+b.Right*p.x+b.Up*p.y;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            AddPrism(vertices,triangles,A(p0),A(p1),A(p2),A(p3),B(p0)-A(p0),B(p1)-A(p1),B(p2)-A(p2),B(p3)-A(p3));
            var mesh=MeshFrom("Bank-following gallery shell span",vertices,triangles);meshes.Add(mesh);
            Add(Vector3.zero,Vector3.one,Quaternion.identity,list,mesh);
        }

        static void AddPrism(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d,
            Vector3 az,Vector3 bz,Vector3 cz,Vector3 dz)
        {
            Vector3 center=(a+b+c+d+a+az+b+bz+c+cz+d+dz)/8f;
            AddFace(v,t,a,b,c,d,center);AddFace(v,t,a+az,d+dz,c+cz,b+bz,center);
            AddFace(v,t,a,a+az,b+bz,b,center);AddFace(v,t,b,b+bz,c+cz,c,center);
            AddFace(v,t,c,c+cz,d+dz,d,center);AddFace(v,t,d,d+dz,a+az,a,center);
        }
        static void AddFace(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector3 center)
        {
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),(a+b+c+d)*.25f-center)<0){var swap=b;b=d;d=swap;}
            int n=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);
            t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n);t.Add(n+2);t.Add(n+3);
        }
        static Mesh MeshFrom(string name,List<Vector3> vertices,List<int> triangles)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        static void Add(Vector3 p,Vector3 size,Quaternion q,List<CombineInstance> list,Mesh mesh){list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,q,size)});}
        void Combine(string name,List<CombineInstance> instances,Material material,ShadowCastingMode castShadows=ShadowCastingMode.On){var mesh=new Mesh{name=name};mesh.CombineMeshes(instances.ToArray());meshes.Add(mesh);var go=new GameObject(name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=castShadows;}
        void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);}
    }
}
