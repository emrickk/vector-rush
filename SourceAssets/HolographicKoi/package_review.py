"""Build a review from real native captures, keeping the rejected koi comparison."""
import argparse,json,subprocess
from pathlib import Path
from PIL import Image,ImageDraw
p=argparse.ArgumentParser();p.add_argument('--capture',type=Path,required=True);p.add_argument('--before',type=Path,required=True);p.add_argument('--stills',type=Path);p.add_argument('--output',type=Path,required=True);a=p.parse_args();out=a.output.resolve();out.mkdir(parents=True,exist_ok=True)
now=json.loads((a.capture/'preview.json').read_text());before=json.loads((a.before/'preview.json').read_text());assert now['complete']
stillFolder=a.stills or a.capture;stillData=json.loads((stillFolder/'preview.json').read_text());assert stillData['complete'] and stillData['buildGuid']==now['buildGuid']
records=[]
for label,progress in [('reveal',.50),('hero',.55),('underneath',.585)]:
 for kind,data,folder in [('new',stillData,stillFolder),('old',before,a.before)]:
  f=min(data['frames'],key=lambda s:abs(s['progress']-progress));name=f'{label}-{kind}.jpg';Image.open(folder/f['file']).convert('RGB').save(out/name,quality=95)
  if kind=='new':records.append(dict(label=label,progress=f['progress'],seconds=f['seconds'],image=name,source=f['file']))
# Matching native-image crops; no synthetic anatomy or retouching.
for kind in ['old','new']:
 Image.open(out/f'hero-{kind}.jpg').crop((485,120,985,330)).save(out/f'head-{kind}.jpg',quality=95)
strip=Image.new('RGB',(1920,390));draw=ImageDraw.Draw(strip)
for i,r in enumerate(records):
 strip.paste(Image.open(out/r['image']).resize((640,360)),(i*640,0));draw.text((i*640+10,370),f"ACTUAL NATIVE GAMEPLAY / {r['seconds']:.1f}s",fill='white')
strip.save(out/'native-progression.jpg',quality=95)
reference=Path(__file__).resolve().parent/'Reference/2026-09-17/koi-overhead-target-01.png'
Image.open(reference).convert('RGB').save(out/'reference.jpg',quality=95)
start=min(now['frames'],key=lambda f:abs(f['progress']-.46))['seconds']-.3
end=min(now['frames'],key=lambda f:abs(f['progress']-.65))['seconds']+.3
duration=end-start
subprocess.run(['ffmpeg','-y','-hide_banner','-loglevel','warning','-ss',str(start),'-i',str(a.capture/'full-lap.mp4'),'-t',str(duration),'-c:v','libx264','-crf','18','-preset','fast','-fps_mode','vfr','-c:a','aac','-movflags','+faststart',str(out/'luminous-turn.mp4')],check=True)
(out/'shots.json').write_text(json.dumps(dict(buildGuid=now['buildGuid'],shots=records,excerptStart=start,excerptDuration=duration),indent=2)+'\n')
(out/'index.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush · Luminous koi rebuild</title><style>*{box-sizing:border-box}body{margin:0;background:#0c1218;color:#f4ece7;font:16px system-ui}main{max-width:1440px;margin:auto;padding:32px}h1{font-size:clamp(32px,4vw,56px);letter-spacing:-.04em;margin:.4em 0}p{color:#bac8cd;line-height:1.6;max-width:1000px}.eyebrow{font-size:12px;letter-spacing:.16em;color:#ffab85}video,img{display:block;width:100%}button{font:inherit;background:#f5a47e;border:0;border-radius:6px;padding:12px 20px;margin-top:12px;cursor:pointer;color:#20130f}.grid{display:grid;grid-template-columns:1fr 1fr;gap:20px}figure{margin:0;background:#18242b}figcaption{padding:14px}.note{font-size:13px}h2{margin-top:36px;font-size:24px}@media(max-width:700px){main{padding:16px}.grid{grid-template-columns:1fr}}</style><main><div class="eyebrow">VECTOR RUSH / REBUILT CITY LANDMARK</div><h1>A more natural head and body.</h1><p>The koi has fuller head and shoulder proportions, a defined jaw and belly, flush eyes and a shallow mouth. The body tapers gradually into the tail. The layered fins, swimming behavior and lower flyover are retained.</p><video id="turn" controls playsinline loop preload="auto" src="luminous-turn.mp4" poster="hero-new.jpg" aria-label="Actual native driving beneath the rebuilt luminous koi"></video><button onclick="const v=document.querySelector('video');v.paused?v.play():v.pause()">Play / pause native turn</button><p class="note">Actual standalone Unity capture with game audio and automated steering. Video at 1280×720; comparison stills at 1920×1080. The excerpt uses recorded frame timestamps; it is not a performance benchmark.</p><h2>Previous koi / rebuilt koi</h2><div class="grid"><figure><img src="hero-old.jpg" alt="Previous koi head and body proportions"><figcaption>Previous native koi — revision 07</figcaption></figure><figure><img src="hero-new.jpg" alt="Rebuilt luminous koi with flowing fins and a wet-road reflection"><figcaption>Head and body correction — revision 08</figcaption></figure></div><h2>Head detail</h2><div class="grid"><figure><img src="head-old.jpg" alt="Native crop of previous protruding eye and pointed mouth"><figcaption>Revision 07 · native crop</figcaption></figure><figure><img src="head-new.jpg" alt="Native crop of revised cheek, eye and jaw"><figcaption>Revision 08 · same image crop</figcaption></figure></div><h2>Art target</h2><figure><img src="reference.jpg" alt="Scenario-generated koi polish target"><figcaption>Generated reference · guides the koi art, not a native gameplay frame</figcaption></figure><h2>Through the approach</h2><img src="native-progression.jpg" alt="Three actual driving frames during the approach to the luminous koi"><p class="note">One landmark rebuilt. Course, collisions, handling, ship, HUD and weather are preserved. Artistic acceptance remains with the owner.</p></main></html>''')
print(out/'index.html')
