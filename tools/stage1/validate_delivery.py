"""Validate actual native captures without altering poses, pixels or metadata."""
import argparse,json,math,hashlib
from pathlib import Path
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('baseline',type=Path);p.add_argument('candidate',type=Path);p.add_argument('motion',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
def read(root):
 d=json.loads((root/'opening-evidence.json').read_text());assert d['complete'] and len(d['frames'])==432
 for label in ('01-approach','02-bend','03-reveal'):
  with Image.open(root/(label+'.png')) as im:assert im.size==(1920,1080);im.verify()
 return d
b,c,m=read(a.baseline),read(a.candidate),read(a.motion)
def dist(v,w):return math.sqrt(sum((v[k]-w[k])**2 for k in ('x','y','z')))
def angle(v,w):
 v=list(v.values());w=list(w.values());dot=abs(sum(x*y for x,y in zip(v,w)))/math.sqrt(sum(x*x for x in v)*sum(x*x for x in w))
 return math.degrees(2*math.acos(min(1,dot)))
rows=[]
for x,y in zip(b['frames'],c['frames']):
 rows.append(dict(cameraMeters=dist(x['cameraPosition'],y['cameraPosition']),cameraDegrees=angle(x['cameraRotation'],y['cameraRotation']),fovDegrees=abs(x['fov']-y['fov']),progress=abs(x['progress']-y['progress']),speedKph=abs(x['speed']-y['speed']),racerMeters=max(dist(v,w) for v,w in zip(x['racerPositions'],y['racerPositions']))))
maxima={k:max(row[k] for row in rows) for k in rows[0]}
anchors=[dict(name=x['anchor'],progress=y['progress'],cameraPosition=y['cameraPosition'],cameraRotation=y['cameraRotation'],fov=y['fov'],difference=rows[i]) for i,(x,y) in enumerate(zip(b['frames'],c['frames'])) if x.get('anchor')]
for f in m['frames']:
 with Image.open(a.motion/f['file']) as im:assert im.size==(1920,1080);im.verify()
assert maxima['cameraMeters']==0 and maxima['fovDegrees']==0 and maxima['progress']==0 and maxima['racerMeters']==0
assert m['buildGuid']==c['buildGuid']
report=dict(status='CAPTURES_VERIFIED',baselineBuild=b['buildGuid'],candidateBuild=c['buildGuid'],artRevision=c['revision'],framesPerCapture=432,simulationSeconds=18,simulationHz=24,width=1920,height=1080,comparisonMaxima=maxima,anchors=anchors,motionFramesVerified=432,scope='Native screenshots. Same ordinary physics and automated inputs. Camera position, FOV, path and racer positions checked for all frames. Camera rotation differences quantified separately. Motion is 24 Hz simulation-time, silent, not real-time performance or human driving.')
a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,indent=2))
