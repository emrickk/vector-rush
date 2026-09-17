using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace VectorRush
{
    // Opt-in fixed-camera native proof. The race simulation and ordinary material clock remain live.
    [DefaultExecutionOrder(32000)]
    public sealed class BillboardMotionEvidence : MonoBehaviour
    {
        [Serializable] sealed class Frame{public string file;public float seconds,animationTime;public int sign;}
        [Serializable] sealed class Report{public string buildGuid;public string scope="Native fixed-camera animated displays, real-time Time.time; no manual driving or performance claim.";public List<Frame> frames=new List<Frame>();public bool complete;}
        Vector3 position;Quaternion rotation;bool locked;
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-billboardEvidence");if(flag<0){enabled=false;yield break;}
            string folder=args[flag+1];if(!Path.IsPathRooted(folder)||Directory.Exists(folder))throw new IOException("Use a fresh absolute billboard evidence folder");Directory.CreateDirectory(folder);Directory.CreateDirectory(Path.Combine(folder,"frames"));
            yield return new WaitForSecondsRealtime(3);
            var owner=VectorBootstrap.Instance;var board=GetComponent<AnimatedBillboards>();
            var hud=FindFirstObjectByType<RaceHUD>();if(hud)hud.enabled=false;
            owner.Camera.GetComponent<ChaseCamera>().enabled=false;
            var report=new Report{buildGuid=Application.buildGUID};float start=Time.realtimeSinceStartup;int serial=0;
            foreach(int sign in new[]{0,1,3,6,8}){
                var target=board.displays[sign];var renderer=target.GetComponent<MeshRenderer>();float height=renderer.bounds.size.y;
                Vector3 aim=target.position+(sign==8?Vector3.up*4:sign==1?Vector3.down*3:Vector3.zero);
                position=aim-target.forward*(height*(sign==8?3.5f:sign==1?1.8f:1.5f))+target.right*(height*.12f);
                if(sign==3){aim=target.position+new Vector3(-3,0,3);position=aim+new Vector3(24,2,24);}
                rotation=Quaternion.LookRotation(aim-position,Vector3.up);locked=true;owner.Camera.fieldOfView=48;
                float stop=Time.realtimeSinceStartup+(sign==1?11:6);float next=0;
                while(Time.realtimeSinceStartup<stop){
                    yield return new WaitForEndOfFrame();
                    if(Time.realtimeSinceStartup<next)continue;next=Time.realtimeSinceStartup+1f/12;
                    string file="frames/frame-"+(serial++).ToString("D5")+".png";ScreenCapture.CaptureScreenshot(Path.Combine(folder,file));
                    report.frames.Add(new Frame{file=file,seconds=Time.realtimeSinceStartup-start,animationTime=Time.time,sign=sign});
                }
            }
            report.complete=true;File.WriteAllText(Path.Combine(folder,"motion.json"),JsonUtility.ToJson(report,true));yield return new WaitForSecondsRealtime(.5f);Application.Quit();
        }
        void LateUpdate(){if(locked){var c=VectorBootstrap.Instance.Camera.transform;c.SetPositionAndRotation(position,rotation);}}
    }
}
