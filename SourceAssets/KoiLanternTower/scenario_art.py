"""Scenario concept and enamel surface source; credentials stay in the environment."""
import json, os, sys, urllib.request
from pathlib import Path
ROOT=Path(__file__).resolve().parent/'scenario'
BRIEFS={
 'concept': (1536,1024,'''Production concept sheet for ONE original Koi Lantern Tower landmark in a rainy high-end cyberpunk racing city. Beautiful believable 3D architectural sculpture, isolated complete object, three quarter front view, dark neutral teal backdrop. A gigantic elegant ivory porcelain koi fish with vermilion patches curls diagonally upward in front of a thin circular amber lantern ring. Distinct recognizable fish anatomy, rounded head, dark glass eyes, whiskers, layered translucent bronze fins, broad split flowing tail, patterned ceramic scales. The ring is open in the center, no flat image panel. Exposed bronze structural joints and cables suspend the sculpture physically from a deep graphite metal arch frame. Entire sculpture and ring about 22 metres wide, on the roof of a narrow 55-metre-tall market tower with stacked warm glass windows, projecting roof canopy, vertical copper ribs and maintenance balcony. Architecture elegant and restrained, buildable structural forms, sophisticated amber ivory vermilion palette against charcoal, striking silhouette readable from 150 metres away. No cars, road, people, text, logos, watermark or extra floating objects. Soft studio illumination revealing geometry and surface materials, restrained emission with no blown-out glow. Asset design reference, not gameplay.'''),
 'enamel': (1024,1024,'''Flat seamless material texture, square, edge to edge, no object or perspective. Premium ivory koi-fish porcelain enamel with small overlapping scalloped fish scales. Thin warm bronze-grey outlines, milky ivory centers, subtle fine ceramic crazing and soft roughness variation. A few broad organic vermilion red patches crossing several scales, about 18 percent red overall; restrained warm gold flecks. Refined handcrafted architectural ceramic sculpture material. Uniform neutral lighting, no shadows, no bloom, no text, no borders. Scales consistent size, about 12 columns and 18 offset rows across image. Tileable on all edges.''')}
def api(path,body=None):
 req=urllib.request.Request('https://api.cloud.scenario.com/v1'+path,data=None if body is None else json.dumps(body).encode(),headers={'Authorization':'Basic '+os.environ['SCENARIO_API_TOKEN'],'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=120) as response:return json.load(response)
for name,(w,h,prompt) in BRIEFS.items():
 folder=ROOT/name;folder.mkdir(parents=True,exist_ok=True)
 if sys.argv[1]=='submit':
  if (folder/'job.json').exists():continue
  body=dict(prompt=prompt,width=w,height=h,numOutputs=1,quality='high',background='opaque')
  (folder/'request.json').write_text(json.dumps(body,indent=2)+'\n')
  job=api('/generate/custom/model_openai-gpt-image-2',body)['job']['jobId']
  (folder/'job.json').write_text(json.dumps(dict(provider='Scenario',model='model_openai-gpt-image-2',jobId=job),indent=2)+'\n');print(name,job,flush=True)
 else:
  if (folder/'art.png').exists():print(name,'saved');continue
  job=api('/jobs/'+json.loads((folder/'job.json').read_text())['jobId'])['job'];print(name,job['status'],flush=True)
  if job['status']=='success':
   aid=job['metadata']['assetIds'][0];asset=api('/assets/'+aid)['asset']
   with urllib.request.urlopen(asset['url'],timeout=120) as response:(folder/'art.png').write_bytes(response.read())
   (folder/'asset.json').write_text(json.dumps(dict(assetId=aid),indent=2)+'\n')
