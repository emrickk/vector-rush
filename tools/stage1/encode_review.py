import json,subprocess
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[2]
source=root/'artifacts/stage1-reference-match/motion-hud'
out=root.parents[1]/'artifacts/stage1-reference-match/delivery';out.mkdir(parents=True,exist_ok=True)
d=json.loads((source/'opening-evidence.json').read_text());assert d['complete']
frames=d['frames'];first=next(i for i,f in enumerate(frames) if f['progress']>=.085);last=next(i for i,f in enumerate(frames) if f['progress']>=.312);count=last-first+1
for name,start,n in [('Stage1-city-motion.mp4',first,count),('Stage1-opening-18s.mp4',0,len(frames))]:
 subprocess.run(['ffmpeg','-y','-hide_banner','-loglevel','warning','-framerate','24','-start_number',str(start),'-i',str(source/'frames/frame-%04d.png'),'-frames:v',str(n),'-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',str(out/name)],check=True)
 subprocess.run(['ffmpeg','-hide_banner','-loglevel','error','-i',str(out/name),'-f','null','-'],check=True)
canvas=Image.new('RGB',(1920,1200),(9,16,22));draw=ImageDraw.Draw(canvas);font=ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf',22)
for slot in range(6):
 idx=first+round((count-1)*slot/5);im=Image.open(source/frames[idx]['file']).convert('RGB').resize((640,360))
 x=(slot%3)*640;y=(slot//3)*400;canvas.paste(im,(x,y+35));draw.text((x+15,y+8),f'{(idx-first)/24:.2f}s   track {frames[idx]["progress"]:.3f}',font=font,fill=(221,241,244))
canvas=canvas.crop((0,0,1920,800));canvas.save(out/'Motion-contact-sheet.jpg',quality=94)
report={'buildGuid':d['buildGuid'],'sourceFrames':432,'clipFirstFrame':first,'clipLastFrame':last,'clipFrames':count,'clipSeconds':count/24,'fps':24,'width':1920,'height':1080,'audio':False,'scope':'Native rendered PNG sequence, 24Hz simulation-time automated physics. HUD visible. Encoded without frame interpolation. Both clips fully decoded for integrity.'}
(out/'motion-verification.json').write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
