"""Render the review only from verified final-candidate evidence."""
from pathlib import Path
from string import Template
import json, shutil, html, xml.etree.ElementTree as ET
r=Path(__file__).resolve().parents[2];e=r/'artifacts/architecture';out=r.parents[1]/'artifacts/architecture-review';checks=out/'checks';checks.mkdir(exist_ok=True)
read=lambda p:json.loads(p.read_text())
identity=read(e/'build-09/build-identity.json');capture=read(e/'capture-verification.json');motion=read(out/'motion.json')
before=read(e/'performance-baseline/performance.json');after=read(e/'performance-candidate/performance.json');race=read(e/'full-race/validation.json');tests=ET.parse(e/'editmode-results.xml').getroot()
assert tests.attrib['failed']=='0' and tests.attrib['passed']=='248'
assert race['status']=='COMPLETE' and len(race['races'])==1
result=race['races'][0];assert len(result['finishers'])==6 and not any(result['recoveries'])
assert identity['buildGuid']==capture['candidateBuild']==motion['buildGuid']==after['buildGuid']==race['buildGuid']
assert all(not any(anchor['difference'].values()) for anchor in capture['anchors'])
for src,name in [(e/'build-09/build-identity.json','build-identity.json'),(e/'capture-verification.json','capture-verification.json'),(e/'performance-baseline/performance.json','performance-baseline.json'),(e/'performance-candidate/performance.json','performance-candidate.json'),(e/'full-race/validation.json','full-race.json'),(e/'editmode-results.xml','editmode-results.xml')]:shutil.copy2(src,checks/name)
rows=[]
for i,(label,detail) in enumerate([('Approach','Deep frames, grouped office glazing and a continuous glass spine.'),('Bend','Distinct façade systems and selective signage preserve the racing sightline.'),('Exit','Lower city blocks, foundations and stepped silhouettes fill the previously empty view.')],1):
 rows.append(f'''<article class="anchor"><h3>{i:02d} / {label}</h3><p>{detail}</p><div class="compare"><img src="images/after-{i}.png" alt="New native {label.lower()} screenshot"><img class="old" src="images/before-{i}.png" alt="Prior native {label.lower()} screenshot"><div class="labels"><span>BEFORE / CITY 05</span><span>AFTER / ARCHITECTURE 09</span></div></div><div class="controls"><button data-value="100">Before</button><input type="range" min="0" max="100" value="50" aria-label="{label} before and after comparison"><button data-value="0">After</button></div><figcaption><a href="images/before-{i}.png">Original before PNG</a> · <a href="images/after-{i}.png">Original after PNG</a></figcaption></article>''')
values=dict(comparisons=''.join(rows),seconds=f'{motion["seconds"]:.2f}',guid=identity['buildGuid'],tests=f'{tests.attrib["passed"]} / {tests.attrib["total"]} passed',beforemean=f'{before["meanMs"]:.2f}',beforep95=f'{before["p95Ms"]:.2f}',aftermean=f'{after["meanMs"]:.2f}',afterp95=f'{after["p95Ms"]:.2f}',race=f'All six racers finished; zero recoveries.<br>Player: {result["playerTime"]:.2f} seconds.',app=html.escape(identity['appPath']))
page=Template((r/'tools/stage1/architecture_review_template.html').read_text()).substitute(values)
(out/'City Architecture Review.html').write_text(page)
print(out/'City Architecture Review.html')
