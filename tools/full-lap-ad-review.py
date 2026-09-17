#!/usr/bin/env python3
"""Package inspected native full-lap ad footage with playable chapter links."""
import argparse, html, json, os
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps

p=argparse.ArgumentParser(description=__doc__)
p.add_argument('--capture',type=Path,required=True)
p.add_argument('--output',type=Path,required=True)
a=p.parse_args();capture=a.capture.resolve();out=a.output.resolve();out.mkdir(parents=True,exist_ok=True)
d=json.loads((capture/'preview.json').read_text());assert d['complete']
assert (capture/'full-lap.mp4').is_file()
chapters=[('Opening landmark',.04),('Animated portrait and ticker',.235),('After Hours',.29),('Animated corner pair',.59),('Night Market',.666),('Interior triptych',.754),('Tunnel display and ticker',.869),('Last Train',.934)]
records=[];cards=[]
for i,(name,progress) in enumerate(chapters):
 f=min(d['frames'],key=lambda f:abs(f['progress']-progress));dest=f'ad-{i+1:02}.jpg'
 with Image.open(capture/f['file']) as im:im.convert('RGB').save(out/dest,quality=93)
 records.append(dict(name=name,seconds=f['seconds'],progress=f['progress'],source=f['file'],image=dest))
 cards.append(f'<article><button class="jump" data-time="{max(0,f["seconds"]-1.5):.3f}"><img src="{dest}" alt="Native gameplay: {html.escape(name)}"><span>{html.escape(name)} · {f["seconds"]:.1f}s</span></button></article>')
(out/'chapters.json').write_text(json.dumps(dict(buildGuid=d['buildGuid'],chapters=records),indent=2)+'\n')
hero=Image.new('RGB',(1920,384));draw=ImageDraw.Draw(hero)
for column,i in enumerate([2,4,7]):
 with Image.open(out/records[i]['image']) as im:hero.paste(ImageOps.fit(im,(640,360)),(column*640,0))
 draw.text((column*640+12,366),f"{records[i]['name']} | native gameplay | {records[i]['seconds']:.1f}s",fill='white')
hero.save(out/'three-artworks-native.jpg',quality=94)
video=html.escape(os.path.relpath(capture/'full-lap.mp4',out),quote=True)
(out/'index.html').write_text(f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush · Full-lap advertising</title><style>
*{{box-sizing:border-box}}body{{margin:0;background:#0b1218;color:#e8f3f4;font:16px system-ui}}main{{max-width:1500px;margin:auto;padding:32px}}h1{{font-size:clamp(32px,4vw,56px);letter-spacing:-.04em;margin:.3em 0}}p{{line-height:1.65;max-width:950px;color:#b8cbd0}}.eyebrow{{font-size:12px;letter-spacing:.15em;color:#75dedb}}video{{width:100%;background:black;display:block}}.grid{{display:grid;grid-template-columns:repeat(2,1fr);gap:20px;margin-top:28px}}button{{background:#142129;border:1px solid #30474e;border-radius:8px;padding:0;overflow:hidden;color:inherit;cursor:pointer;text-align:left;width:100%;font:inherit}}button:hover{{border-color:#71e0d6}}img{{display:block;width:100%}}span{{display:block;padding:14px}}.note{{font-size:13px}}@media(max-width:700px){{.grid{{grid-template-columns:1fr}}main{{padding:18px}}}}</style></head><body><main>
<div class="eyebrow">VECTOR RUSH / NATIVE GAMEPLAY</div><h1>Advertisements across the lap.</h1>
<p>After Hours, Night Market and Last Train are now placed in the playable scene. The animated campaigns extend beyond the opening, with smaller displays fitted inside the later corridor. Select an image to jump to its approach in the full lap.</p>
<video id="lap" controls preload="auto" src="{video}" aria-label="Full native driving lap with distributed advertisements"></video>
<div class="grid">{''.join(cards)}</div>
<p class="note">Actual standalone Unity capture with automated steering, game audio and recorded frame timestamps. Images show the driving camera. Capture is not a performance benchmark or manual driving test. Build {d['buildGuid']}.</p>
</main><script>const lap=document.querySelector('#lap');for(const button of document.querySelectorAll('.jump'))button.addEventListener('click',async()=>{{lap.pause();if(lap.readyState<2)await new Promise(resolve=>lap.addEventListener('loadeddata',resolve,{{once:true}}));const sought=new Promise(resolve=>lap.addEventListener('seeked',resolve,{{once:true}}));lap.currentTime=Number(button.dataset.time);await sought;lap.play();lap.scrollIntoView({{behavior:'smooth',block:'center'}});}});</script></body></html>''')
print(out/'index.html')
