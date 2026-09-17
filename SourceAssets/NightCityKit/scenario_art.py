"""Scenario billboard art. Credential comes only from SCENARIO_API_TOKEN."""
from pathlib import Path
import os,sys,json,urllib.request
ROOT=Path(__file__).resolve().parent
OUT=ROOT/'scenario';OUT.mkdir(exist_ok=True)
BRIEFS={
 'afterhours':(1024,1536,'''Production billboard texture for a richly detailed rainy cyberpunk racing city. Flat graphic artwork edge to edge, no mockup, no frame, no perspective, no photographed billboard. Premium avant-garde future fashion editorial: a close-up three-quarter profile of an androgynous East Asian adult wearing sculptural translucent icy-cyan eyewear and a black technical rain jacket. Dramatic ivory face against deep petrol-teal, razor-thin scarlet accent, sculpted photographic detail and subtle analog halftone. Strong asymmetric Swiss editorial grid with large exact title "AFTER HOURS" at bottom, tiny exact "NIGHT EDITION / 07" at top. Portrait fills the upper two-thirds, restrained tasteful typography; high-end art direction, striking silhouette legible from far away, fine garment texture visible up close. No other words, no real brand or watermark. Sharp texture, no baked neon bloom, no motion blur.'''),
 'koi_market':(1536,1024,'''Finished landscape advertising artwork, flat edge-to-edge image for a luminous cyberpunk night-market sign. A beautifully painted gigantic ivory koi fish curves around a deep red rising sun on dark charcoal petrol background. Japanese woodblock-inspired scales with refined modern silkscreen texture, tiny teal wave pattern and cream geometric framing. Large exact words "NIGHT MARKET" on the left and exact small "OPEN ALL NIGHT" below, fish dominates right side, restrained editorial typography. Premium original fictional city advertisement, rich detailed scales and paper grain reward close inspection, bold recognizable composition at distance. No real logos, no perspective, no billboard mockup, no border outside artwork, no bloom, no motion blur. Polished print design, not generic futuristic HUD.'''),
 'transit':(1024,1536,'''Finished premium portrait travel advertisement texture for a futuristic city, flat graphic artwork edge to edge. Monumental elegant silver elevated train gliding between abstract teal towers, strong diagonal composition, cinematic photographic detail blended with precise mid-century travel-poster geometry. Deep petrol black background, pale ivory train, vivid vermilion vertical block, sparse cyan highlights. Exact title "LAST TRAIN" in large condensed ivory typography, exact small subtitle "THE CITY NEVER SLEEPS". Fine architectural linework, gorgeous tonal depth, limited sophisticated palette. Original fictional city artwork with no existing logos, no other text, no photographed sign or mockup, no perspective distortion of the poster surface, no baked glow or blur. Print-ready sharp detail.''')}
def api(path,body=None):
 req=urllib.request.Request('https://api.cloud.scenario.com/v1'+path,data=None if body is None else json.dumps(body).encode(),headers={'Authorization':'Basic '+os.environ['SCENARIO_API_TOKEN'],'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=90) as r:return json.load(r)
for name,(w,h,prompt) in BRIEFS.items():
 folder=OUT/name;folder.mkdir(exist_ok=True)
 if sys.argv[1]=='submit':
  if (folder/'job.json').exists():continue
  body=dict(prompt=prompt,width=w,height=h,numOutputs=1,quality='high',background='opaque')
  (folder/'request.json').write_text(json.dumps(body,indent=2))
  job=api('/generate/custom/model_openai-gpt-image-2',body)['job']['jobId']
  (folder/'job.json').write_text(json.dumps(dict(provider='Scenario',model='model_openai-gpt-image-2',jobId=job),indent=2));print(name,job,flush=True)
 else:
  if (folder/'art.png').exists():print(name,'already saved');continue
  j=api('/jobs/'+json.loads((folder/'job.json').read_text())['jobId'])['job'];print(name,j['status'],flush=True)
  if j['status']=='success':
   for aid in j.get('metadata',{}).get('assetIds',[]):
    a=api('/assets/'+aid)['asset']
    with urllib.request.urlopen(a['url'],timeout=90) as r:(folder/'art.png').write_bytes(r.read())
    (folder/'asset.json').write_text(json.dumps(dict(assetId=aid),indent=2))
