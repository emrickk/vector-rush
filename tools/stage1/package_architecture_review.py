"""Package checked native architecture captures; reference concepts stay labeled."""
import argparse, json, shutil, subprocess, hashlib
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
p=argparse.ArgumentParser();p.add_argument("candidate");a=p.parse_args()
r=Path(__file__).resolve().parents[2];e=r/"artifacts/architecture";out=r.parents[1]/"artifacts/architecture-review";out.mkdir(parents=True,exist_ok=True)
media=out/"images";media.mkdir(exist_ok=True)
source=e/a.candidate;motion=e/(a.candidate+"-motion")
baseline=r/"artifacts/stage1-reference-match/native-05"
for i,name in enumerate(["01-approach","02-bend","03-reveal"],1):
 shutil.copy2(baseline/(name+".png"),media/f"before-{i}.png")
 shutil.copy2(source/(name+".png"),media/f"after-{i}.png")
ref=r.parents[1]/"artifacts/stage1-reference-match/reference-packet"
for i,name in enumerate(["01-city-road-lighting.jpg","02-ship-exhaust-atmosphere.jpg"],1):shutil.copy2(ref/name,media/f"concept-{i}.jpg")
d=json.loads((motion/"opening-evidence.json").read_text());assert d["complete"] and len(d["frames"])==432
first=next(i for i,f in enumerate(d["frames"]) if f["progress"]>=.085)
last=next(i for i,f in enumerate(d["frames"]) if f["progress"]>=.32)
video=out/"City-architecture-motion.mp4"
subprocess.run(["ffmpeg","-y","-hide_banner","-loglevel","error","-framerate","24","-start_number",str(first),"-i",str(motion/"frames/frame-%04d.png"),"-frames:v",str(last-first+1),"-c:v","libx264","-preset","fast","-crf","18","-pix_fmt","yuv420p","-movflags","+faststart",str(video)],check=True)
subprocess.run(["ffmpeg","-hide_banner","-loglevel","error","-i",str(video),"-f","null","-"],check=True)
font=ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf",23)
# Three matched anchors, each original remains separately accessible.
comparison=Image.new("RGB",(1920,1770),(9,16,22));draw=ImageDraw.Draw(comparison)
for row,name in enumerate(["Approach","Bend","Exit"]):
 for col,label in enumerate(["before","after"]):
  x=col*960;y=row*590;draw.text((x+20,y+12),f"{name.upper()} / {label.upper()}",font=font,fill=(220,237,240))
  comparison.paste(Image.open(media/f"{label}-{row+1}.png").convert("RGB").resize((960,540)),(x,y+50))
comparison.save(out/"Before-after.jpg",quality=94)
manifest={"buildGuid":d["buildGuid"],"revision":d["revision"],"clipFrames":last-first+1,"fps":24,"seconds":(last-first+1)/24,"audio":False,"scope":"Native rendered PNG sequence using ordinary chase camera and automated physics. Silent simulation-time capture, not real-time performance or human driving."}
(out/"motion.json").write_text(json.dumps(manifest,indent=2)+"\n")
print(out)
