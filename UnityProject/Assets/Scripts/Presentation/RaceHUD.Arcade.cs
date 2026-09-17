using UnityEngine;

namespace VectorRush
{
    public sealed partial class RaceHUD
    {
        // Reference A uses a 1672 x 941 design canvas. Art is separate from live text/fills.
        Texture2D arcadePanel, arcadeSpeedPanel, arcadeCell, arcadeBolt;
        GUIStyle arcadeType;
        Font arcadeHeavy, arcadeMedium;
        float arcadeBoost;
        public float ArcadeBoostAmount => arcadeBoost;
        readonly Color arcadeCyan = new Color(.12f,.94f,1f);
        void PrepareArcadeArt()
        {
            if (arcadePanel) return;
            arcadePanel=PolygonArt(512,128,new[]{new Vector2(48,2),new Vector2(510,2),new Vector2(463,126),new Vector2(2,126)},false);
            arcadeSpeedPanel=PolygonArt(512,128,new[]{new Vector2(105,2),new Vector2(510,2),new Vector2(440,126),new Vector2(2,126)},false);
            arcadeCell=PolygonArt(128,64,new[]{new Vector2(34,4),new Vector2(124,4),new Vector2(96,60),new Vector2(5,60)},true);
            arcadeBolt=PolygonArt(48,72,new[]{new Vector2(33,1),new Vector2(5,42),new Vector2(22,42),new Vector2(12,71),new Vector2(44,29),new Vector2(28,29)},true);
            arcadeHeavy=Resources.Load<Font>("Fonts/Saira-BlackItalic");
            arcadeMedium=Resources.Load<Font>("Fonts/Saira-MediumItalic");
            arcadeType=new GUIStyle { font=arcadeHeavy?arcadeHeavy:regular, fontStyle=FontStyle.Normal, alignment=TextAnchor.MiddleLeft, clipping=TextClipping.Clip, padding=new RectOffset() };
        }
        static Texture2D PolygonArt(int w,int h,Vector2[] points,bool luminous)
        {
            var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name=luminous?"HUD A energy":"HUD A graphite panel",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            var pixels=new Color[w*h];
            for(int y=0;y<h;y++) for(int x=0;x<w;x++)
            {
                var p=new Vector2(x+.5f,y+.5f); bool inside=false; float distance=float.MaxValue;
                for(int i=0,j=points.Length-1;i<points.Length;j=i++)
                {
                    Vector2 a=points[i],b=points[j],ab=b-a;
                    if((a.y>p.y)!=(b.y>p.y) && p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x) inside=!inside;
                    distance=Mathf.Min(distance,Vector2.Distance(p,a+ab*Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude)));
                }
                float alpha=inside?Mathf.Clamp01(distance+.5f):Mathf.Clamp01(.5f-distance);
                Color c;
                if(luminous) c=new Color(.23f,.96f,1f,alpha);
                else {
                    float sheen=Mathf.Pow(1-y/(float)h,9)*.018f;
                    c=new Color(.0015f+sheen,.004f+sheen,.009f+sheen,alpha*.98f);
                    if(inside && distance<1.6f) c=new Color(.04f,.075f,.10f,alpha*.8f);
                }
                pixels[(h-1-y)*w+x]=c;
            }
            texture.SetPixels(pixels);texture.Apply(false,true);return texture;
        }
        public static float ArcadeCellFill(float charge,int index) => Mathf.Clamp01(Mathf.Clamp01(charge)*5-index);
        public static float ArcadeBoostTarget(float speed, bool boosting, bool reduced, bool racing)
            => reduced || !racing || !boosting ? 0f : Mathf.Lerp(.25f,1f,Mathf.InverseLerp(175f,260f,speed));
        public static float ArcadeBoostStep(float current,float target,float dt,bool suppressed)
            => suppressed ? 0f : Mathf.Lerp(current,target,1f-Mathf.Exp(-(target>current?10f:6f)*Mathf.Max(0f,dt)));
        public static Vector2 ArcadeBoostOffset(float amount,float seconds)
            => Mathf.Clamp01(amount)*new Vector2(2.2f*Mathf.Sin(seconds*107f),1.1f*Mathf.Sin(seconds*151f+.7f));
        void UpdateArcadeBoost(float speed)
        {
            bool suppressed=preferences.ReducedInterfaceMotion || director.Phase!=RacePhase.Racing;
            float target=ArcadeBoostTarget(speed,director.Player && director.Player.IsBoosting,preferences.ReducedInterfaceMotion,director.Phase==RacePhase.Racing);
            arcadeBoost=ArcadeBoostStep(arcadeBoost,target,Time.deltaTime,suppressed);
        }
        void ArcadeLabel(string value,Rect rect,int size,bool heavy=true,Color? color=null)
        {
            arcadeType.fontSize=size;arcadeType.fontStyle=FontStyle.Normal;
            arcadeType.font=(heavy?arcadeHeavy:arcadeMedium)??regular;
            arcadeType.normal.textColor=color??new Color(.94f,.98f,1f);
            GUI.Label(rect,value,arcadeType);
        }
        void ArcadeEdge(Vector2 a,Vector2 b,float thickness=3)
        {
            Line(a,b,thickness+7,new Color(.04f,.65f,1f,.10f));
            Line(a,b,thickness+3,new Color(.08f,.85f,1f,.22f));
            Line(a,b,thickness,arcadeCyan);
        }
        void DrawArcadeTelemetry()
        {
            PrepareArcadeArt();PrepareCircuit();
            Matrix4x4 original=GUI.matrix;
            float s=Mathf.Min(width/1672f,height/941f);
            float w=width/s,h=height/s;
            GUI.matrix=original*Matrix4x4.Scale(new Vector3(s,s,1));
            // HUD scaling remains anchored at the respective screen corner.
            var saved=ScaleHud(new Vector2(22,52));
            Graphic(arcadePanel,new Rect(39,64,340,94),Color.white);
            ArcadeEdge(new Vector2(24,136),new Vector2(49,85),4);
            ArcadeEdge(new Vector2(49,85),new Vector2(83,54),4);
            ArcadeEdge(new Vector2(83,54),new Vector2(131,54),4);
            ArcadeLabel(director.Position.ToString(),new Rect(70,65,82,88),80);
            ArcadeLabel("/"+RacerCount,new Rect(142,100,80,47),36,false);
            Line(new Vector2(244,82),new Vector2(225,142),1.5f,new Color(.5f,.73f,.8f));
            ArcadeLabel("LAP",new Rect(271,77,66,25),19,false,muted);
            ArcadeLabel(Mathf.Clamp(director.Lap,1,director.TotalLaps)+"/"+director.TotalLaps,new Rect(264,104,85,43),34);
            GUI.matrix=saved;
            saved=ScaleHud(new Vector2(w-35,55));
            float tx=w-304;
            Graphic(arcadePanel,new Rect(tx,55,276,59),Color.white);
            ArcadeEdge(new Vector2(w-44,57),new Vector2(w-65,104),4);
            ArcadeLabel(TimeLabel(director.RaceTime),new Rect(tx+39,57,218,52),34,false);
            GUI.matrix=saved;
            saved=ScaleHud(new Vector2(w-34,h-53));
            float x=w-469,y=h-209;
            float motion=preferences.ReducedInterfaceMotion?0f:arcadeBoost;
            Vector2 offset=ArcadeBoostOffset(motion,Time.time);
            GUI.matrix=GUI.matrix*Matrix4x4.Translate(new Vector3(offset.x,offset.y,0));
            // Directional afterglow stays behind the sharp foreground instruments.
            if(motion>.005f)
            {
                Line(new Vector2(x+14-28*motion,y+85+3*motion),new Vector2(x+84,y+8),6,new Color(.08f,.86f,1f,.22f*motion));
                Line(new Vector2(x+353-20*motion,y+154+3*motion),new Vector2(x+428,y+54),7,new Color(.08f,.86f,1f,.18f*motion));
            }
            Graphic(arcadeSpeedPanel,new Rect(x+5,y,394,95),Color.white);
            Graphic(arcadePanel,new Rect(x,y+89,436,66),Color.white);
            ArcadeEdge(new Vector2(x+14,y+85),new Vector2(x+84,y+8),4);
            ArcadeEdge(new Vector2(x+84,y+8),new Vector2(x+99,y+2),3);
            ArcadeEdge(new Vector2(x+371,y+148),new Vector2(x+428,y+54),5);
            ArcadeEdge(new Vector2(x+353,y+154),new Vector2(x+371,y+148),3);
            string speedText=Mathf.RoundToInt(displayedSpeed).ToString();
            if(motion>.005f)
            {
                ArcadeLabel(speedText,new Rect(x+98-10*motion,y-4+2*motion,208,98),85,true,new Color(.1f,.9f,1f,.11f*motion));
                ArcadeLabel(speedText,new Rect(x+98-5*motion,y-4+motion,208,98),85,true,new Color(.1f,.9f,1f,.18f*motion));
            }
            ArcadeLabel(speedText,new Rect(x+98,y-4,208,98),85);
            ArcadeLabel("KM/H",new Rect(x+282,y+39,86,42),21,false,muted);
            Graphic(arcadeBolt,new Rect(x+39,y+98,29,46),Color.white);
            float charge=director.Player?director.Player.Boost01:0;
            bool release=director.Player&&director.Player.BoostRequiresRelease;
            Color energy=release||charge<.15f?amber:Color.white;
            for(int i=0;i<5;i++)
            {
                var rect=new Rect(x+78+i*53,y+105,64,36);
                Graphic(arcadeCell,rect,new Color(.17f,.30f,.34f,.75f));
                float fill=ArcadeCellFill(charge,i);
                if(fill<=0)continue;
                GUI.BeginGroup(new Rect(rect.x,rect.y,rect.width*fill,rect.height));
                Graphic(arcadeCell,new Rect(0,0,rect.width,rect.height),energy);
                GUI.EndGroup();
            }
            if(release) ArcadeLabel("RELEASE",new Rect(x+170,y+154,180,28),16,false,amber);
            GUI.matrix=saved;
            saved=ScaleHud(new Vector2(72,h-65));
            DrawArcadeMap(h);
            GUI.matrix=saved;
            // Brief state notices only; no persistent rival/ghost/debug paragraphs.
            if(collisionCue>.08f && uiCollision) Graphic(uiCollision,new Rect(w*.5f-18,120,36,36),new Color(1,.7f,.3f,collisionCue));
            else if(lapNotice>0) ArcadeLabel(director.Lap==director.TotalLaps?"FINAL LAP":"LAP "+director.Lap,new Rect(w*.5f-95,125,210,42),28);
            if(positionNotice>0) { var icon=positionMessage=="POSITION GAINED"?uiRankUp:uiRankDown;if(icon) Graphic(icon,new Rect(73,166,28,28),Color.white); }
            GUI.matrix=original;
        }
        void DrawArcadeMap(float h)
        {
            if(circuitPoints==null)return;
            float scale=165f/Mathf.Max(circuitMax.x-circuitMin.x,circuitMax.y-circuitMin.y);
            Vector2 center=(circuitMin+circuitMax)*.5f;
            Vector2 Map(Vector3 p)=>new Vector2(151,h-153)+(new Vector2(p.x,-p.z)-center)*scale;
            for(int i=1;i<circuitPoints.Length;i++) {
                var a=Map(new Vector3(circuitPoints[i-1].x,0,-circuitPoints[i-1].y));var b=Map(new Vector3(circuitPoints[i].x,0,-circuitPoints[i].y));
                Line(a,b,5,ink);Line(a,b,2.5f,Color.white);
            }
            var start=circuit.Evaluate(0);var gate=Map(start.Position);
            Line(gate+Vector2.up*5,gate-Vector2.up*5,2,Color.white);
            foreach(var racer in director.Racers) {
                if(!racer)continue;var p=Map(racer.transform.position);
                if(racer!=director.Player){Box(p.x-3,p.y-3,6,6,Color.white);continue;}
                var f=racer.transform.forward;var forward=new Vector2(f.x,-f.z).normalized;var right=new Vector2(-forward.y,forward.x);
                Line(p+forward*8,p-forward*5+right*5,3,acid);Line(p+forward*8,p-forward*5-right*5,3,acid);
            }
        }
        void DisposeArcadeArt(){if(arcadePanel)Destroy(arcadePanel);if(arcadeSpeedPanel)Destroy(arcadeSpeedPanel);if(arcadeCell)Destroy(arcadeCell);if(arcadeBolt)Destroy(arcadeBolt);}
    }
}
