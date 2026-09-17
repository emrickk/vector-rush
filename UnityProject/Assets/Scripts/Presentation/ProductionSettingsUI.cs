using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VectorRush
{
    public sealed class ProductionSettingsUI : MonoBehaviour
    {
        public static bool IsOpen {get;private set;}
        static ProductionSettingsUI instance;
        int processedFrame=-1, ownedFrame=-1, actionFrame=-1;
        int focus,rebinding=-1;string notice="";RaceDirector director;
        // Every menu consumer dispatches settings first. Script Update order cannot
        // expose an opening/closing press to the menu beneath the modal.
        public static bool BlocksRaceInput
        {
            get
            {
                if(!instance)return false;
                instance.ProcessInput();
                return IsOpen||instance.ownedFrame==Time.frameCount;
            }
        }
        readonly string[] actions={"Steer left","Steer right","Throttle","Brake","Boost","Recover","Left airbrake","Right airbrake"};
        public void Initialize(RaceDirector race){director=race;instance=this;IsOpen=false;rebinding=-1;}
        void SetOpen(bool open)
        {
            IsOpen=open;rebinding=-1;notice="";
            ownedFrame=actionFrame=Time.frameCount;
        }
        void Update()=>ProcessInput();
        void ProcessInput()
        {
            if(!isActiveAndEnabled||!director||processedFrame==Time.frameCount)return;
            processedFrame=Time.frameCount;
            if(director.Phase==RacePhase.Racing||director.Phase==RacePhase.Countdown)
            {
                if(IsOpen||rebinding>=0)SetOpen(false);
                return;
            }
            bool toggle=Pressed(KeyCode.F1);
#if ENABLE_INPUT_SYSTEM
            toggle|=Gamepad.current!=null&&Gamepad.current.buttonNorth.wasPressedThisFrame;
#endif
            if(toggle){SetOpen(!IsOpen);return;}
            if(!IsOpen)return;
            ownedFrame=Time.frameCount;
            bool cancel=Pressed(KeyCode.Escape);
#if ENABLE_INPUT_SYSTEM
            cancel|=Gamepad.current!=null&&Gamepad.current.buttonEast.wasPressedThisFrame;
#endif
            if(cancel)
            {
                actionFrame=Time.frameCount;
                if(rebinding>=0){rebinding=-1;notice="Binding cancelled";}
                else SetOpen(false);
                return;
            }
            if(rebinding>=0)
            {
                foreach(KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
                    if(Allowed(code)&&Pressed(code))
                    {
                        actionFrame=Time.frameCount;
                        if(PlayerPreferences.Current.TryRebind(BindingAction(rebinding),code,out string error)){PlayerPreferences.Current.Save();rebinding=-1;notice="Binding saved";}
                        else notice=error;
                        return;
                    }
                return;
            }
            int vertical=Pressed(KeyCode.DownArrow)?1:Pressed(KeyCode.UpArrow)?-1:0;
            int horizontal=Pressed(KeyCode.RightArrow)?1:Pressed(KeyCode.LeftArrow)?-1:0;
            bool accept=Pressed(KeyCode.Return)||Pressed(KeyCode.KeypadEnter);
#if ENABLE_INPUT_SYSTEM
            var pad=Gamepad.current;
            if(pad!=null){if(pad.dpad.down.wasPressedThisFrame)vertical=1;if(pad.dpad.up.wasPressedThisFrame)vertical=-1;if(pad.dpad.right.wasPressedThisFrame)horizontal=1;if(pad.dpad.left.wasPressedThisFrame)horizontal=-1;accept|=pad.buttonSouth.wasPressedThisFrame;}
#endif
            focus=(focus+vertical+15)%15;
            if(vertical!=0||horizontal!=0||accept)actionFrame=Time.frameCount;
            if(horizontal!=0&&focus<4)Adjust(focus,horizontal*.05f);
            if(accept)Activate(focus);
        }
        void Adjust(int index,float amount)
        {
            var p=PlayerPreferences.Current;
            switch(index)
            {
                case 0:p.SetVolumes(p.MasterVolume+amount,p.MusicVolume,p.EffectsVolume);break;
                case 1:p.SetVolumes(p.MasterVolume,p.MusicVolume+amount,p.EffectsVolume);break;
                case 2:p.SetVolumes(p.MasterVolume,p.MusicVolume,p.EffectsVolume+amount);break;
                case 3:p.SetSteeringSensitivity(p.SteeringSensitivity+amount);break;
            }
            p.Save();
        }
        void Activate(int index)
        {
            if(index==4){var p=PlayerPreferences.Current;p.SetShake(!p.ShakeEnabled);p.Save();}
            else if(index>=5&&index<=12){rebinding=index-5;notice="Press an unused letter, Space or Shift. Escape / B cancels.";}
            else if(index==13){PlayerPreferences.Current.ResetDefaults();PlayerPreferences.Current.Save();notice="Defaults restored";}
            else if(index==14)SetOpen(false);
        }
        void OnGUI()
        {
            if(!director||director.Phase==RacePhase.Racing||director.Phase==RacePhase.Countdown)return;
            ProcessInput();
            GUI.depth=-10;
            // Keyboard/controller actions belong to ProcessInput. IMGUI's focused
            // buttons must not replay Enter/Space after Update already handled it.
            if(!IsOpen){if(MouseButton(new Rect(Screen.width-190,22,166,34),"SETTINGS  F1 / Y"))SetOpen(true);return;}
            float scale=Mathf.Min(Screen.width/1000f,Screen.height/850f);var matrix=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float width=Screen.width/scale,height=Screen.height/scale,x=(width-620)/2;
            GUI.Box(new Rect(x,15,620,height-30),"PLAYER SETTINGS");
            var p=PlayerPreferences.Current;float[] values={p.MasterVolume,p.MusicVolume,p.EffectsVolume,p.SteeringSensitivity};string[] labels={"Master volume","Music volume","Effects volume","Steering sensitivity"};
            for(int i=0;i<15;i++)
            {
                float y=60+i*43;GUI.color=i==focus?new Color(.7f,1,.25f):Color.white;
                if(i<4){GUI.Label(new Rect(x+25,y,210,30),labels[i]+"  "+values[i].ToString("0.00"));
                    if(MouseButton(new Rect(x+270,y,50,30),"−")){focus=i;Adjust(i,-.05f);}if(MouseButton(new Rect(x+340,y,50,30),"+")){focus=i;Adjust(i,.05f);}}
                else
                {
                    string label=i==4?"Camera shake: "+(p.ShakeEnabled?"ON":"OFF"):i<=12?actions[i-5]+": "+p.BindingFor(BindingAction(i-5)):i==13?"Reset defaults":"Back to race menu";
                    if(MouseButton(new Rect(x+25,y,570,32),(rebinding==i-5&&rebinding>=0?"PRESS KEY — ":"")+label)){focus=i;Activate(i);}
                }
            }
            GUI.color=Color.white;GUI.Label(new Rect(x+25,720,570,60),notice+"\nArrow keys / D-pad to select and adjust · Enter / A to activate · Y / F1 to close");GUI.matrix=matrix;
        }
        bool MouseButton(Rect rect,string label)
        {
            bool mouseRelease=Event.current.type==EventType.MouseUp&&Event.current.button==0;
            bool clicked=GUI.Button(rect,label);
            if(!clicked||!mouseRelease||actionFrame==Time.frameCount)return false;
            actionFrame=ownedFrame=Time.frameCount;
            return true;
        }
        static PlayerAction BindingAction(int index)
        {
            PlayerAction[] map={PlayerAction.SteerLeft,PlayerAction.SteerRight,PlayerAction.Throttle,PlayerAction.Brake,
                PlayerAction.Boost,PlayerAction.Recover,PlayerAction.AirbrakeLeft,PlayerAction.AirbrakeRight};
            return map[Mathf.Clamp(index,0,map.Length-1)];
        }
        static bool Allowed(KeyCode code)
        {
            int value=(int)code;
            return code==KeyCode.Space||code==KeyCode.LeftShift||code==KeyCode.RightShift||
                (value>=(int)KeyCode.A&&value<=(int)KeyCode.Z)||(value>=(int)KeyCode.Alpha0&&value<=(int)KeyCode.Alpha9);
        }
        static bool Pressed(KeyCode code)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if(Input.GetKeyDown(code))return true;
#endif
#if ENABLE_INPUT_SYSTEM
            if(Keyboard.current==null)return false;
            string name=code.ToString();
            if(name=="Return")name="Enter";else if(name=="KeypadEnter")name="NumpadEnter";
            else if(name.StartsWith("Alpha",System.StringComparison.Ordinal))name="Digit"+name.Substring(5);
            return System.Enum.TryParse(name,true,out Key key)&&Keyboard.current[key].wasPressedThisFrame;
#else
            return false;
#endif
        }
        void OnDisable(){if(instance==this)SetOpen(false);}
        void OnDestroy(){if(instance==this){IsOpen=false;instance=null;}}
    }
}
