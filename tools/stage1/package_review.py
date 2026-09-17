"""Package review media and primary verification records, preserving original pixels."""
import json,shutil,subprocess,hashlib,html
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parents[2];e=root/'artifacts/stage1-reference-match'
shared=root.parents[1]/'artifacts/stage1-reference-match';out=shared/'delivery';out.mkdir(parents=True,exist_ok=True)
media=out/'images';media.mkdir(exist_ok=True);checks=out/'checks';checks.mkdir(exist_ok=True)
identity=json.loads((e/'build-05/build-identity.json').read_text());verified=json.loads((e/'delivery-verification.json').read_text())
bp=json.loads((e/'performance-baseline/performance.json').read_text());cp=json.loads((e/'performance-candidate/performance.json').read_text())
race=json.loads((e/'full-race/validation.json').read_text());assert race['status']=='COMPLETE'
assert len(race['races'])==1 and len(race['races'][0]['finishers'])==6 and not any(race['races'][0]['recoveries'])
commit=subprocess.check_output(['git','rev-parse','HEAD'],cwd=root,text=True).strip()
for i,name in enumerate(['01-approach','02-bend','03-reveal'],1):
 shutil.copy2(e/'baseline-matched'/(name+'.png'),media/f'before-{i}.png');shutil.copy2(e/'native-05'/(name+'.png'),media/f'native-{i}.png')
shutil.copy2(e/'motion-hud/01-approach.png',media/'native-hud.png')
for i,name in enumerate(['01-city-road-lighting.jpg','02-ship-exhaust-atmosphere.jpg'],1):shutil.copy2(shared/'reference-packet'/name,media/f'reference-{i}.jpg')
for source,name in [(e/'build-05/build-identity.json','build-identity.json'),(e/'delivery-verification.json','capture-verification.json'),(e/'editmode-results-02.xml','editmode-results.xml'),(e/'performance-baseline/performance.json','performance-baseline.json'),(e/'performance-candidate/performance.json','performance-candidate.json'),(e/'full-race/validation.json','full-race.json'),(shared/'reference-packet/sources.json','reference-sources.json')]:shutil.copy2(source,checks/name)
font=ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf',27)
canvas=Image.new('RGB',(1920,1180),(9,18,24));d=ImageDraw.Draw(canvas)
for x,y,file,label in [(0,0,'reference-1.jpg','SCENARIO TARGET  /  CITY + ROAD'),(960,0,'native-1.png','NATIVE GAMEPLAY  /  FIXED APPROACH'),(0,590,'reference-2.jpg','SCENARIO TARGET  /  SHIP + EXHAUST'),(960,590,'native-2.png','NATIVE GAMEPLAY  /  FIXED BEND')]:
 d.text((x+20,y+12),label,font=font,fill=(210,241,245));im=Image.open(media/file).convert('RGB').resize((960,540));canvas.paste(im,(x,y+50))
canvas.save(out/'Reference-and-game.png')
canvas=Image.new('RGB',(1920,590),(9,18,24));d=ImageDraw.Draw(canvas)
for x,file,label in [(0,'before-1.png','BEFORE  /  SAME NATIVE CAMERA'),(960,'native-1.png','STAGE 1  /  SAME NATIVE CAMERA')]:
 d.text((x+20,12),label,font=font,fill=(210,241,245));canvas.paste(Image.open(media/file).convert('RGB').resize((960,540)),(x,50))
canvas.save(out/'Before-and-after.png')
readme=f'''# Vector Rush: Stage 1 city review

A playable banked city passage is ready for review. It has denser architecture, an illuminated graphite road, clearer turn guidance and fuller blue exhaust. It moves toward the two selected Scenario targets; it does not claim an exact visual match.

Open **Stage1-review.html** for the references, native comparisons and an 8.54-second moving clip. The original native PNGs are in `images/`. Clean comparison captures hide the HUD; `native-hud.png` and both clips show the ordinary HUD.

## Playable build

- App: `{identity['appPath']}`
- Build GUID: `{identity['buildGuid']}`
- Source milestone: `{commit}` on `stage1/astra-reference-city`.
- Default controls: W / Up throttle, A-D / Left-Right steer, S / Down brake, Space boost, Escape pause. Existing remappings remain supported.

## What is implemented

The new persistent Unity scene includes 140 building footprints across foreground, lower city and distant skyline layers; stepped towers, paired shafts, framed ads, rooftop plant, service structures, bank-following rails, supports and direction-correct chevrons. Cyan/magenta emission, local lights and a surface response make the road readable. The original ship geometry uses the delivered finish maps, masked boost emission, brighter nozzle cores and broader layered blue particles. Track geometry, driving rules and the chase camera remain unchanged. The full course remains playable; the art pass is bounded to the opening city, approximately progress 0 to 0.32.

## Verification

- Strict import: all 30 corrected FBX LOD models pass the unchanged bounds/material contract.
- Tests: **248 / 248 passed**, zero failures or skips.
- Captures: all 432 native 1920 × 1080 motion frames verified. The three chosen before/after stills have exact recorded camera position, rotation, FOV and racer-position parity. Across all 432 frames, position/FOV/path/speed/racer positions are identical; maximum non-anchor camera rotation difference is {verified['comparisonMaxima']['cameraDegrees']:.4f}°.
- Motion: 8.54-second excerpt plus full 18-second sequence, 24 Hz simulation time, automated normal physics, silent, no frame interpolation. Both videos fully decoded. This is separate from real-time performance or human driving.
- Opening performance, isolated on Apple M4 Max at 1920 × 1080: before mean {bp['meanMs']:.3f} ms / P95 {bp['p95Ms']:.3f} ms / P99 {bp['p99Ms']:.3f} ms; candidate mean {cp['meanMs']:.3f} ms / P95 {cp['p95Ms']:.3f} ms / P99 {cp['p99Ms']:.3f} ms. These are delivered frame intervals under the existing 120 fps cap, not isolated GPU costs.
- One complete real-time automated race: all six racers finished, zero recoveries; player time {race['races'][0]['playerTime']:.3f} seconds, post-finish result and pause state held. See the original records in `checks/`.

## Remaining gaps and scope

Foreground facade/material variation remains more stylized and repetitive than the generated targets. Rail response combines local lights and an authored surface approximation; full dynamic city reflections are not implemented. The busy left facade competes with existing HUD text. The video target's complete motion treatment, later UI polish and full-circuit art rollout remain outside this step. Human handling/controller comfort are unverified. The earlier report's third-consecutive-race timeout was not revalidated in this visual pass, which checks one complete race.

The two Scenario images are concept targets, not gameplay. Their ship and route differ from the retained native geometry; the original video remains the route/banking authority. Artistic acceptance remains with the owner.
'''
(out/'README.md').write_text(readme)
(root/'docs/stage1-reference-match-review.md').write_text(readme.replace('Open **Stage1-review.html**','Open the review package at `../../../artifacts/stage1-reference-match/delivery/Stage1-review.html`'))
page=f'''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush · Stage 1 city review</title><style>
*{{box-sizing:border-box}}body{{margin:0;background:#071017;color:#e2ecef;font:16px/1.55 -apple-system,BlinkMacSystemFont,Arial,sans-serif}}main{{max-width:1500px;margin:auto;padding:48px 32px 70px}}h1{{font-size:42px;letter-spacing:-1.5px;line-height:1.12;margin:12px 0 18px}}h2{{font-size:24px;margin:52px 0 16px}}p{{max-width:880px;color:#b4c7cf}}.tag{{color:#70e6ed;letter-spacing:3px;font-size:12px;font-weight:700}}.grid{{display:grid;grid-template-columns:1fr 1fr;gap:18px}}figure{{margin:0;background:#101e27;border:1px solid #29404d;border-radius:8px;overflow:hidden}}img,video{{display:block;width:100%;height:auto}}figcaption{{padding:12px 16px;color:#b8d0d9;font-size:14px}}strong{{color:#eaf7fa}}.hero{{margin-top:30px}}.note{{font-size:13px;color:#8ca8b4}}a{{color:#72e4ec}}table{{border-collapse:collapse;width:100%;max-width:880px}}td,th{{text-align:left;padding:10px 16px;border-bottom:1px solid #29404d}}th{{color:#82d8e1}}button{{background:#142c39;border:1px solid #3b606f;color:#dceff5;padding:10px 16px;border-radius:5px;cursor:pointer;margin:0 6px 15px 0}}button.on{{background:#1d626e;border-color:#78dae4}}li{{max-width:1050px;margin-bottom:10px;color:#b4c7cf}}footer{{margin-top:45px;border-top:1px solid #29404d;padding-top:20px;font-size:12px;color:#829eaa}}@media(max-width:760px){{main{{padding:28px 15px}}h1{{font-size:32px}}.grid{{grid-template-columns:1fr}}}}
</style><main><div class="tag">VECTOR RUSH / NATIVE UNITY REVIEW</div><h1>A banked city with light, depth and direction.</h1><p>A playable Stage 1 passage is ready for review: denser city layers, graphite road with cyan/magenta response, turn chevrons, framed service architecture and fuller blue exhaust. The original handling, chase camera and recognizable ship are preserved.</p>
<figure class="hero"><img src="images/native-1.png" alt="Native Unity city bend"><figcaption><strong>Native gameplay.</strong> Fixed approach at progress 0.150885. HUD hidden for the clean comparison; ordinary HUD shown in the motion clip below.</figcaption></figure>
<h2>The selected targets and the actual game</h2><div class="grid"><figure><img src="images/reference-1.jpg"><figcaption><strong>Scenario concept target.</strong> City enclosure, dark road and colored rail light.</figcaption></figure><figure><img src="images/native-1.png"><figcaption><strong>Native Unity.</strong> Banked approach, real geometry and existing ship.</figcaption></figure><figure><img src="images/reference-2.jpg"><figcaption><strong>Scenario concept target.</strong> Ship finish, blue exhaust and depth.</figcaption></figure><figure><img src="images/native-2.png"><figcaption><strong>Native Unity.</strong> Fixed bend with layered skyline and fuller exhaust.</figcaption></figure></div><p class="note">Concept images are visual targets. Their ship and road geometry differ from the native game. This pass does not claim an exact match.</p>
<h2>Before and after from the same camera</h2><div id="choices"><button class="on" data-n="1">Approach · 0.151</button><button data-n="2">Bend · 0.220</button><button data-n="3">Reveal · 0.300</button></div><div class="grid"><figure><img id="before" src="images/before-1.png"><figcaption>Preserved A01 build · fresh native capture</figcaption></figure><figure><img id="after" src="images/native-1.png"><figcaption>Stage 1 candidate · fresh native capture</figcaption></figure></div><p class="note">The three selected stills have identical recorded camera position, rotation, FOV and racer positions. Across all 432 frames, camera position/FOV and racer paths remain identical; non-anchor camera rotation differs by at most 0.336°.</p>
<h2>8.54 seconds through the city</h2><figure><video controls preload="metadata" poster="images/native-hud.png" src="Stage1-city-motion.mp4"></video><figcaption>Native gameplay with the ordinary HUD. 205 frames at 24 Hz simulation time, automated normal physics, silent. No frame interpolation.</figcaption></figure><p class="note"><a href="Stage1-opening-18s.mp4">Full 18-second capture</a> · <a href="Motion-contact-sheet.jpg">Sequence contact sheet</a> · <a href="Reference-and-game.png">Reference comparison image</a> · <a href="Before-and-after.png">Before/after image</a></p>
<h2>Verified checks</h2><p><strong>248 / 248 tests passed.</strong> All 30 corrected city models pass the strict import contract. Every motion frame was verified and both clips fully decoded. One real-time automated race completed with all six finishers and zero recoveries; the post-finish result and pause state held.</p><table><thead><tr><th>Opening frame intervals</th><th>Before</th><th>Stage 1</th></tr></thead><tbody><tr><td>Mean</td><td>{bp['meanMs']:.3f} ms</td><td>{cp['meanMs']:.3f} ms</td></tr><tr><td>P95</td><td>{bp['p95Ms']:.3f} ms</td><td>{cp['p95Ms']:.3f} ms</td></tr><tr><td>P99</td><td>{bp['p99Ms']:.3f} ms</td><td>{cp['p99Ms']:.3f} ms</td></tr></tbody></table><p class="note">Separate real-time 18-second runs at 1920 × 1080 on Apple M4 Max, under the existing 120 fps cap. Delivered frame intervals, not isolated GPU costs or human handling.</p>
<h2>What still needs work</h2><ul><li>Near facade and material variation remain more stylized and repetitive than the references.</li><li>Colored road response uses local lights and an authored surface approximation. Full dynamic city reflections and the reference video's complete motion treatment remain open.</li><li>Busy left-side buildings compete with the existing HUD. Later UI polish and full-circuit art rollout are outside this Stage 1 passage.</li></ul><p class="note">Human handling/controller comfort remain unverified. The earlier third-consecutive-race timeout was not revalidated here; this pass checks one complete race. Artistic acceptance remains with the owner.</p>
<footer><a href="README.md">Build path, controls and methodology</a> · <a href="checks/capture-verification.json">Capture verification</a> · <a href="checks/editmode-results.xml">Test results</a> · <a href="checks/full-race.json">Race record</a><br>Build {identity['buildGuid']} · Source {commit[:12]} · Stage 1 only</footer></main><script>document.querySelectorAll('button[data-n]').forEach(b=>b.onclick=()=>{{document.querySelectorAll('button').forEach(x=>x.classList.toggle('on',x===b));document.getElementById('before').src='images/before-'+b.dataset.n+'.png';document.getElementById('after').src='images/native-'+b.dataset.n+'.png';}});</script></html>'''
(out/'Stage1-review.html').write_text(page)
manifest={'buildGuid':identity['buildGuid'],'sourceCommit':commit,'files':[{'path':str(p.relative_to(out)),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(out.rglob('*')) if p.is_file() and p.name!='checksums.json']}
(out/'checksums.json').write_text(json.dumps(manifest,indent=2)+'\n');print(out)
