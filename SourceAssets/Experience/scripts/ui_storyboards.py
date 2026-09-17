"""Astra's HUD/menu layout and state timing assets. Static design evidence only."""
from pathlib import Path
import json,shutil
from PIL import Image,ImageDraw,ImageFilter
from package_art import ROOT,dump,font

OUT=ROOT/"packages/vrx-d01"
if (OUT/"READY.json").exists():raise RuntimeError("Immutable revision")
(OUT/"design").mkdir(parents=True,exist_ok=True)
TOKENS={
    "schema":"vrx-art-tokens-1","revision":1,"referenceSize":[1920,1080],
    "safeArea":{"horizontal":48,"vertical":36},
    "font":{"display":"vrx_font_display_a","text":"vrx_font_text_a"},
    "pixels":{"speed":84,"rank":70,"lapTime":30,"label":20,"menuRow":28,"menuTitle":64},
    "tracking":{"display":1,"event":2,"body":0},
    "colorsSrgb":{"background":"#091724","primary":"#DCEEF5","muted":"#8CAAB8",
                  "guidance":"#58D5FF","boost":"#58D5FF","impact":"#FFB057","sign":"#D94487"},
    "hud":{"rank":{"anchor":"top-left","offset":[48,36]},
           "raceTime":{"anchor":"top-right","offset":[48,36]},
           "map":{"anchor":"bottom-left","offset":[48,36]},
           "speedBoost":{"anchor":"bottom-right","offset":[48,36],"size":[360,210]},
           "clearCentre":{"normalizedRect":[.26,.17,.48,.58]}},
    "menu":{"rows":["START RACE","SETTINGS","CONTROLS","QUIT APPLICATION"],
            "rowHeight":64,"rowGap":12,"left":80,"top":480,"width":560},
    "motion":{"clock":"UI unscaled; freeze event progression while paused; reset on restart/return/recovery",
              "boost":{"enterMs":80,"peakMs":160,"settleMs":240,"exitMs":180,"scalePeak":1.035,"travelPx":6},
              "rank":{"enterMs":110,"holdMs":450,"exitMs":170,"scalePeak":1.06,"travelPx":10,"coalesceMs":180},
              "collision":{"flashMs":35,"edgeWarningMs":300,"travelPx":0},
              "focus":{"enterMs":100,"travelPx":6,"fadeMs":100},
              "reduced":{"travelPx":0,"scalePeak":1,"maxOpacityTransitionMs":100}},
    "behaviour":"Values and state availability come from RaceDirector/HoverVehicle/contacts. Tokens do not create gameplay.",
    "audio":"Reuse current source synthesis. Timbre, envelopes and event coordination in INTEGRATION.md.",
}
dump(OUT/"design/vrx_ui_tokens_a.json",TOKENS)
events={
    "boost":[("0 ms","IsBoosting rises","BOOST",.25),("80 ms","Label and seam arrive","BOOST",.70),
             ("160 ms","Peak emphasis","BOOST",1),("240 ms","Steady sustain","BOOST",.90),
             ("release","State falls","BOOST",.45),("+180 ms","Label clears","",0)],
    "rank":[("0 ms","Position 04 to 03","03",.35),("110 ms","Number emphasis","03",1),
            ("280 ms","Hold","03",.9),("560 ms","Hold ends","03",.9),
            ("650 ms","Arrow fades","03",.45),("730 ms","Stable rank","03",0)],
    "contact":[("enter","Light graze","",.18),("35 ms","Local flash ends","",.08),
               ("stay","Bounded scrape","",.22),("exit","Emission stops","",0),
               ("heavy","One warning onset","CONTACT",1),("+300 ms","Warning clears","",0)]
}
dump(OUT/"design/vrx_event_storyboard_a.json",{"schema":"vrx-event-storyboard-1","events":events,
    "note":"These times describe envelopes following real events; no fabricated gameplay timeline."})

def background(view="entry"):
    im=Image.open(ROOT/"evidence"/("vrx-b01-"+view+"-composition.png")).convert("RGBA").resize((1920,1080))
    overlay=Image.new("RGBA",im.size,(0,8,16,50));im=Image.alpha_composite(im,overlay)
    return im
def paste(im,name,xy,size=None):
    f=ROOT/"packages/vrx-a01/ui"/("vrx_ui_"+name+"_a.png")
    tile=Image.open(f).convert("RGBA")
    if size:tile=tile.resize(size,Image.Resampling.LANCZOS)
    im.alpha_composite(tile,xy)
def text(im,xy,label,size=22,c="#DCEEF5",display=False):
    ImageDraw.Draw(im).text(xy,label,font=font(size,display),fill=c)
im=background()
paste(im,"panel",(36,26),(258,160));text(im,(60,41),"POSITION",20,"#8CAAB8")
text(im,(56,54),"03",70,display=True);text(im,(143,93),"/ 08",26)
text(im,(1570,36),"LAP 01 / 03",30,display=True);text(im,(1597,80),"00:14.280",30,display=True)
paste(im,"panel",(1500,817),(372,213))
text(im,(1521,832),"328",84,display=True);text(im,(1717,889),"KM/H",20,"#8CAAB8")
paste(im,"boost_track",(1524,952),(320,40))
fill=Image.open(ROOT/"packages/vrx-a01/ui/vrx_ui_boost_fill_a.png").resize((320,40)).convert("RGBA")
fill=fill.crop((0,0,231,40));im.alpha_composite(fill,(1524,952))
text(im,(1524,992),"BOOST",20,"#58D5FF");text(im,(1796,992),"72%",20)
# Map shape derives from actual course points, not a newly invented circuit.
course=json.loads((ROOT/"context/course-data.json").read_text())
positions=[f["position"] for f in course["frames"][::6]]
xs=[p[0] for p in positions];zs=[p[2] for p in positions]
scale=min(190/(max(xs)-min(xs)),135/(max(zs)-min(zs)))
pts=[(60+(p[0]-min(xs))*scale,830+(p[2]-min(zs))*scale) for p in positions]
d=ImageDraw.Draw(im);d.line(pts+[pts[0]],fill="#8CAAB8",width=2);d.ellipse((pts[3][0]-4,pts[3][1]-4,pts[3][0]+4,pts[3][1]+4),fill="#58D5FF")
text(im,(59,995),"NOCTURNE",20);text(im,(680,1041),"STATIC UI DESIGN / sample values / Blender background",19,"#8CAAB8")
im.convert("RGB").save(ROOT/"evidence/vrx-d01-hud.png")
menu=background("exit")
shade=Image.new("RGBA",menu.size);sd=ImageDraw.Draw(shade)
for x in range(1450):
    alpha=int(240*max(0,1-x/1550)**.60)
    sd.line((x,0,x,1080),fill=(3,10,17,alpha))
menu=Image.alpha_composite(menu,shade)
text(menu,(80,72),"ANTI-GRAVITY RACING",22,"#58D5FF")
paste(menu,"wordmark",(73,151),(810,143))
text(menu,(80,332),"NOCTURNE CIRCUIT",32,display=True)
text(menu,(80,384),"MIDNIGHT EXHIBITION",20,"#8CAAB8")
for i,label in enumerate(TOKENS["menu"]["rows"]):
    y=480+i*76
    if i==0:paste(menu,"selection",(80,y),(560,64))
    text(menu,(108,y+13),label,28,display=True)
    if i==0:text(menu,(544,y+20),"ENTER",18,"#58D5FF")
text(menu,(80,993),"LOCAL EXHIBITION",20,"#8CAAB8")
text(menu,(1140,1035),"STATIC MENU DESIGN / existing actions",19,"#8CAAB8")
menu.convert("RGB").save(ROOT/"evidence/vrx-d01-menu.png")
board=Image.new("RGB",(1800,1090),"#091724")
text(board,(50,25),"EVENT MOTION / SHARED TIMING",48,display=True)
text(board,(52,85),"State-driven storyboards. Sample timings are art intent for 03/04.",24,"#8CAAB8")
for r,(event,states) in enumerate(events.items()):
    y=156+r*293
    text(board,(50,y),event.upper(),28,"#58D5FF",True)
    for k,(time,desc,label,strength) in enumerate(states):
        x=50+k*288;d=ImageDraw.Draw(board)
        d.rectangle((x,y+46,x+265,y+234),fill="#112A3A")
        text(board,(x+12,y+56),time,21,"#8CAAB8")
        color="#FFB057" if event=="contact" else "#58D5FF"
        if label:text(board,(x+15,y+91),label,50 if len(label)<4 else 38,color,True)
        d.rectangle((x+16,y+158,x+16+int(230*strength),y+164),fill=color)
        text(board,(x+12,y+187),desc,18)
text(board,(50,1050),"Reduced motion: no travel or scaling; immediate state and <=100 ms opacity transition.",23,"#BED0DA")
board.save(ROOT/"evidence/vrx-d01-motion-storyboard.png")
dump(OUT/"experience-assets.json",{"contractVersion":1,"revision":"vrx-d01","assets":[
    {"id":"vrx_ui_tokens_a","path":"design/vrx_ui_tokens_a.json","type":"design-tokens","revision":1,
     "source":"Astra art specification grounded in existing HUD/menu actions","license":"Project-authored"},
    {"id":"vrx_event_storyboard_a","path":"design/vrx_event_storyboard_a.json","type":"motion-specification","revision":1,
     "source":"Astra event timing specifications; no gameplay implementation","license":"Project-authored"}]})
shutil.copy2(ROOT/"design/INTEGRATION.md",OUT/"INTEGRATION.md")
shutil.copy2(__file__,OUT/"ui_storyboards_recipe.py")
print("UI_STORYBOARDS_READY")
