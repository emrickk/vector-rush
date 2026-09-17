"""Package native camera/motion evidence, preserving original frame pixels."""
from pathlib import Path
import json, shutil, subprocess
import xml.etree.ElementTree as ET
from PIL import Image
root = Path(__file__).resolve().parents[2]
evidence = root / 'artifacts/motion'
out = Path('/Users/anping.wang/output/vector-rush-city-light-2026-09-16/motion-03')
media = out / 'media'
media.mkdir(parents=True, exist_ok=True)
reports = {}
for key, folder in [('before','baseline-03'),('after','native-03')]:
    source = evidence / folder
    data = json.loads((source/'opening-evidence.json').read_text())
    assert data['complete'] and len(data['frames']) == 432
    reports[key] = data
    for frame in data['frames']:
        with Image.open(source/frame['file']) as im:
            assert im.size == (1920,1080)
            im.verify()
    for anchor in ['01-approach','02-bend','03-reveal']:
        shutil.copy2(source/(anchor+'.png'),media/(key+'-'+anchor+'.png'))
    subprocess.run(['ffmpeg','-v','error','-y','-framerate','24','-i',str(source/'frames/frame-%04d.png'),'-frames:v','288','-c:v','libx264','-preset','fast','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',str(media/(key+'.mp4'))],check=True)
    subprocess.run(['ffmpeg','-v','error','-i',str(media/(key+'.mp4')),'-f','null','-'],check=True)
assert reports['before']['buildGuid'] == reports['after']['buildGuid']
for x,y in zip(reports['before']['frames'],reports['after']['frames']):
    for key in ['progress','speed','racerPositions','racerRotations','visualPositions','visualRotations']:
        assert x[key] == y[key], key
reduced = json.loads((evidence/'reduced-03/opening-evidence.json').read_text())
assert reduced['complete'] and reduced['reducedMotion']
assert all(f['blurIntensity']==0 and f['viewBlend'] in [0,1] for f in reduced['frames'])
assert max(f['viewBlend'] for f in reduced['frames']) == 1
frames=reports['after']['frames']
assert max(f['viewBlend'] for f in frames)==1 and frames[160]['viewBlend']==0
assert all(f['blurIntensity']==0 for f in frames if 0<f['viewBlend']<1)
assert frames[60]['blurIntensity']>.15
shutil.copy2(evidence/'native-03/frames/frame-0120.png',media/'look-back.png')
shutil.copy2(evidence/'native-03/frames/frame-0101.png',media/'transition.png')
checks=dict(buildGuid=reports['after']['buildGuid'],framesVerifiedPerVariant=432,identicalRacerPhysics=True,reducedMotionVerified=True,transitionBlurSuppressed=True,cruiseBlurIntensity=frames[60]['blurIntensity'],cameraComparison='Camera pose intentionally differs; track, speed, racer and visual poses match exactly for every frame.',scope='1920x1080 native screenshots, 24 Hz simulation-time, automated steering, silent; 12-second review clip. Separate real-time performance measurement.')
for name in ['perf-03','perf-baseline-03']:
    checks[name]=json.loads((evidence/name/'performance.json').read_text())
tests=ET.parse(evidence/'test-results-03-final.xml').getroot().attrib
checks['tests']={k:tests[k] for k in ['result','total','passed','failed','skipped']}
(out/'checks.json').write_text(json.dumps(checks,indent=2))
shutil.copy2(evidence/'reference/timing.jpg',media/'reference-timing.jpg')
shutil.copy2('/Users/anping.wang/output/vector-rush-review-2026-09-15/assets/reference-video.mp4',media/'reference.mp4')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush · Motion and camera review</title><style>*{box-sizing:border-box}body{margin:0;background:#0b1119;color:#dfebf3;font:17px/1.6 system-ui}main{max-width:1500px;margin:auto;padding:40px 24px}h1{font-size:42px;line-height:1.15}h2{font-size:25px}p{max-width:950px;color:#acbece}.eyebrow{color:#65d3df;font-size:13px;letter-spacing:.14em}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px}figure{margin:0}img,video{width:100%;display:block;background:#000;border-radius:6px}figcaption{font-size:14px;color:#a9bdcb;padding:8px 0}section{margin:36px 0}a{color:#79dbe5}button{border:1px solid #65d3df;border-radius:8px;background:#173740;color:white;font:inherit;padding:10px 16px;cursor:pointer;margin:0 8px 16px 0}details{margin:24px 0}@media(max-width:850px){.pair{grid-template-columns:1fr}h1{font-size:32px}}</style><main><div class="eyebrow">VECTOR RUSH / CAMERA & MOTION / 16 SEPTEMBER 2026</div><h1>Read the road. Feel the speed.</h1><p>A lower, tighter chase camera, smoother turn anticipation, and speed-dependent scenery blur. The ship remains the visual anchor. Neon09 lighting and race physics are preserved.</p><section><h2>Same build, same race</h2><button id="play">Play comparison from start</button><button id="pause">Pause both</button><div class="pair"><figure><video id="before" controls preload="auto" poster="media/before-02-bend.png" src="media/before.mp4"></video><figcaption>Before · previous camera and blur behavior</figcaption></figure><figure><video id="after" controls preload="auto" poster="media/after-02-bend.png" src="media/after.mp4"></video><figcaption>Motion03 · look-back demonstrated at 4–5.5 seconds</figcaption></figure></div><p>Both clips use identical racer physics and automated steering. Native 1080p, 24 fps simulation-time capture, silent. These clips show appearance and transitions; performance was measured separately.</p></section><section><h2>Look behind, then return</h2><p>Hold <strong>Tab</strong> or the controller’s <strong>right-stick button</strong> while racing. Release to return to chase view. The short side arc keeps the ship visible; blur is suppressed during the switch. The clip triggers the same camera request automatically for demonstration.</p><div class="pair"><figure><img src="media/transition.png"><figcaption>During the side arc</figcaption></figure><figure><img src="media/look-back.png"><figcaption>Look-back view · ship facing camera</figcaption></figure></div><p>The existing <strong>Reduced Interface Motion</strong> option disables this pass’s blur, uses instant viewpoint changes and fixes the field of view. No preference is saved by the evidence run.</p></section><section><h2>The bend</h2><div class="pair"><figure><img src="media/before-02-bend.png"><figcaption>Before</figcaption></figure><figure><img src="media/after-02-bend.png"><figcaption>Motion03</figcaption></figure></div></section><p><strong>Checks:</strong> 251 tests passed. At 1080p on M4 Max, the new pass averaged 8.42 ms/frame versus 8.33 ms before. The 99th percentile rose from 9.30 to 15.87 ms, so occasional slower frames remain; this is not a locked 120 fps result. Both measurements cover the opening, with the existing 120 fps cap.</p><section><h2>Reference and scope</h2><video controls preload="metadata" src="media/reference.mp4"></video><p>The supplied 10.112-second clip retains a rear chase view in the sampled sequence. It supports the speed blur and banking direction; it does not establish front/rear transition timing. The look-back interaction is a proposed implementation for this game.</p><p>Track layout, handling, ship, exhaust, lighting and race HUD remain unchanged. No new full-circuit art or automatic cinematic camera switching. Human handling and artistic acceptance remain open.</p></section><details><summary>Verification and limitations</summary><p>All 432 frames per variant were decoded. Racer positions, rotations, visual poses, speeds and progress match exactly; camera pose intentionally differs. Reduced-motion capture confirms zero blur and instant view changes. Current-build tests and separate real-time measurements are in <a href="checks.json">checks.json</a>.</p><p>GPU motion vectors provide blur for camera and object movement; this is not a custom ship-exclusion mask. The opening passage was checked, not every obstacle or camera clearance on the whole circuit. BUILD</p></details></main><script>const a=document.querySelector('#after'),b=document.querySelector('#before');document.querySelector('#play').onclick=()=>{a.currentTime=b.currentTime=0;Promise.all([a.play(),b.play()]).catch(()=>{});};document.querySelector('#pause').onclick=()=>{a.pause();b.pause();};</script></html>'''
(out/'Motion review.html').write_text(html.replace('BUILD','Build '+checks['buildGuid']))
print(json.dumps(checks,indent=2))
