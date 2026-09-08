using UnityEngine;

namespace VectorRush
{
    public sealed class AuthoredCity : MonoBehaviour
    {
        public bool Build(WorldBuilder world,TrackPath track)
        {
            var terrace=Resources.Load<GameObject>("Art/Environment/Solstice_TerraceTower_A");
            var split=Resources.Load<GameObject>("Art/Environment/Solstice_SplitTower_B");
            if(!terrace||!split)return false;
            var vegetation=world.MakeMaterial("Terrace olive planting",new Color(.16f,.24f,.085f),.12f);
            var glazing=world.MakeMaterial("Recessed coastal glazing",new Color(.028f,.085f,.11f),.88f,.22f);
            float[] progress={.64f,.705f,.77f,.825f,.17f,.235f,.43f,.53f};
            float[] offsets={112,105,125,118,124,132,150,150};
            float[] scales={1.02f,1.12f,.94f,.84f,1.05f,.88f,.9f,.8f};
            var samples=new Vector3[256];for(int i=0;i<samples.Length;i++)samples[i]=track.Evaluate((float)i/samples.Length).Position;
            for(int i=0;i<progress.Length;i++){
                var frame=track.Evaluate(progress[i]);
                Vector3 center=frame.Position-frame.Right*offsets[i];center.y=.6f;
                float radius=22f*scales[i];
                for(int attempt=0;attempt<12&&!ClearOfCourse(center,radius+45,samples);attempt++)center-=new Vector3(frame.Right.x,0,frame.Right.z).normalized*10;
                if(!ClearOfCourse(center,radius+35,samples))continue;
                var building=Instantiate(i%2==0?terrace:split,transform);building.name=i%2==0?"Terrace harbor tower":"Split harbor tower";
                Vector3 entrance=frame.Position-center;entrance.y=0;
                building.transform.SetPositionAndRotation(center,Quaternion.LookRotation(entrance.normalized,Vector3.up));building.transform.localScale=Vector3.one*scales[i];
                foreach(var renderer in building.GetComponentsInChildren<Renderer>()){
                    var materials=renderer.sharedMaterials;
                    for(int m=0;m<materials.Length;m++){
                        string n=materials[m]?materials[m].name:"Ivory";
                        materials[m]=n.Contains("Vegetation")?vegetation:n.Contains("Glass")?glazing:n.Contains("Graphite")?world.Graphite:n.Contains("Metal")?world.Metal:world.Ivory;
                    }
                    renderer.sharedMaterials=materials;
                }
                var rotation=building.transform.rotation;
                world.Box("Harbor podium foundation",center-Vector3.up*1.4f,new Vector3(37*scales[i],2.6f,39*scales[i]),rotation,world.Graphite);
                world.Box("Waterfront promenade",center-Vector3.up*.15f,new Vector3(36*scales[i],.3f,38*scales[i]),rotation,world.Ivory);
            }
            return true;
        }
        static bool ClearOfCourse(Vector3 point,float distance,Vector3[] samples)
        {
            foreach(var sample in samples){float x=sample.x-point.x,z=sample.z-point.z;if(x*x+z*z<distance*distance)return false;}
            return true;
        }
    }
}
