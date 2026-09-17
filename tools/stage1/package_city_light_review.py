"""Package verified city-light evidence without retouching native images."""
from pathlib import Path
import argparse, json, shutil, subprocess
from PIL import Image, ImageDraw, ImageFont
p=argparse.ArgumentParser();p.add_argument('candidate');p.add_argument('output',type=Path);a=p.parse_args()
r=Path(__file__).resolve().parents[2]; src=r/'artifacts/city-light'/a.candidate; motion=src.with_name(src.name+'-motion')
before=r/'artifacts/architecture/native-09';out=a.output;media=out/'media';media.mkdir(parents=True,exist_ok=True)
d=json.loads((motion/'opening-evidence.json').read_text());assert d['complete'] and len(d['frames'])==432
for i,name in enumerate(['01-approach','02-bend','03-reveal'],1):
 for label,source in [('before',before),('after',src)]:
  with Image.open(source/(name+'.png')) as im:im.verify()
  shutil.copy2(source/(name+'.png'),media/f'{label}-{i}.png')
reference=Path('/Users/anping.wang/output/vector-rush-review-2026-09-15/assets/reference-video.mp4')
if not (media/'reference.mp4').exists():shutil.copy2(reference,media/'reference.mp4')
subprocess.run(['ffmpeg','-v','error','-y','-ss','0.5','-i',str(reference),'-frames:v','1',str(media/'reference.png')],check=True)
subprocess.run(['ffmpeg','-v','error','-y','-framerate','24','-i',str(motion/'frames/frame-%04d.png'),'-frames:v','432','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',str(media/'full-capture.mp4')],check=True)
end=next(i for i,f in enumerate(d['frames']) if f['progress']>=.32)+1
subprocess.run(['ffmpeg','-v','error','-y','-i',str(media/'full-capture.mp4'),'-frames:v',str(end),'-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',str(media/'city-light.mp4')],check=True)
subprocess.run(['ffmpeg','-v','error','-i',str(media/'city-light.mp4'),'-f','null','-'],check=True)
font=ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf',24)
sheet=Image.new('RGB',(1920,1770),(10,16,23));draw=ImageDraw.Draw(sheet)
for row,title in enumerate(['Approach','Bend','Exit']):
 for col,label in enumerate(['before','after']):
  x=col*960;y=row*590;draw.text((x+18,y+12),title+' / '+label.upper(),font=font,fill='white')
  sheet.paste(Image.open(media/f'{label}-{row+1}.png').resize((960,540)),(x,y+50))
sheet.save(out/'Comparison.jpg',quality=94)
sections=''.join(f'<section><h2>{name}</h2><div class="pair"><figure><a href="media/before-{i}.png"><img src="media/before-{i}.png"></a><figcaption>Before · Architecture 09</figcaption></figure><figure><a href="media/after-{i}.png"><img src="media/after-{i}.png"></a><figcaption>Current · City Light 03</figcaption></figure></div></section>' for i,name in enumerate(['Approach','Bend','Exit'],1))
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush · City lighting review</title><style>*{box-sizing:border-box}body{margin:0;background:#0b1119;color:#dfebf3;font:17px/1.6 system-ui}main{max-width:1440px;margin:auto;padding:40px 24px}h1{font-size:42px;line-height:1.15}h2{font-size:25px}p{max-width:850px;color:#acbece}.eyebrow{color:#65d3df;font-size:13px;letter-spacing:.14em}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px}figure{margin:0}img,video{width:100%;display:block;background:#000;border-radius:6px}figcaption{font-size:14px;color:#a9bdcb;padding:8px 0}section{margin:42px 0}a{color:#79dbe5}details{margin:24px 0}summary{cursor:pointer}@media(max-width:800px){.pair{grid-template-columns:1fr}h1{font-size:32px}}</style><main><div class="eyebrow">VECTOR RUSH / 16 SEPTEMBER 2026</div><h1>Bring light back into the existing city.</h1><p>Varied occupied interiors, clearer existing advertisements, slimmer façade frames and refreshed local glass reflections. The same opening passage, with its existing buildings and signs.</p><section><h2>The passage in motion</h2><video controls preload="metadata" poster="media/after-2.png" src="media/city-light.mp4"></video><p>12 seconds of native gameplay with the ordinary HUD and automated steering. All six racers remain in the race; opponents are visible where normal race spacing puts them in view. Silent, 24 fps simulation-time capture; this is not a real-time performance recording. <a href="media/full-capture.mp4">Full 18-second capture, including the transition beyond this art pass.</a></p></section>'''+sections+'''<section><h2>The original video reference</h2><div class="pair"><figure><video controls preload="metadata" poster="media/reference.png" src="media/reference.mp4"></video><figcaption>User-supplied reference · different game and route</figcaption></figure><div><p>The reference's bright window clusters, layered advertising and contrasting light guide this pass. No pedestrians, trains or traffic systems were added.</p><p>The candidate is more readable and occupied than the prior build. It still has more regular architecture and a cleaner road than the reference; glass uses static local city captures. This pass does not claim a complete visual match.</p></div></div></section><details><summary>Capture and verification details</summary><p>Three native comparison anchors use recorded ordinary chase-camera and racer poses. The separate capture-verification.json records measured differences across all 432 frames. Click any still to inspect the original full-resolution PNG.</p><p>Build GUID: BUILD_GUID. See checks.json for current test and performance results. The previous app is preserved. Owner artistic acceptance remains open.</p></details></main></html>'''
(out/'City lighting review.html').write_text(html.replace('BUILD_GUID',d['buildGuid']))
shutil.copy2(r/'artifacts/city-light/capture-verification.json',out/'capture-verification.json')
print(out)
