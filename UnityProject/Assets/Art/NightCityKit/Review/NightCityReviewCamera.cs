using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class NightCityReviewCamera : MonoBehaviour {
 public RenderPipelineAsset pipeline;
 string output; Camera reviewCamera;
 void Awake(){ if(pipeline){GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;}Application.targetFrameRate=60;reviewCamera=GetComponent<Camera>();var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-nightCityCapture");if(i>=0&&i+1<args.Length)output=args[i+1]; }
 IEnumerator Start(){
  DynamicGI.UpdateEnvironment();
  if(string.IsNullOrEmpty(output))yield break;
  Directory.CreateDirectory(output);
  SetView(false);for(int i=0;i<90;i++)yield return new WaitForEndOfFrame();
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"unity-assembly.png"));
  for(int i=0;i<10;i++)yield return new WaitForEndOfFrame();
  SetView(true);for(int i=0;i<60;i++)yield return new WaitForEndOfFrame();
  ScreenCapture.CaptureScreenshot(Path.Combine(output,"unity-rooftop.png"));
  for(int i=0;i<20;i++)yield return new WaitForEndOfFrame();
  File.WriteAllText(Path.Combine(output,"native-capture.txt"),"Standalone Unity asset viewer. Warmed 90 frames before assembly capture; 60 after camera change. No racing or speed blur.\n");Application.Quit();
 }
 void SetView(bool close){transform.position=close?new Vector3(5,10,16):new Vector3(29,19,35);transform.LookAt(close?new Vector3(-7,4,.5f):new Vector3(-1,10,-8));}
}
