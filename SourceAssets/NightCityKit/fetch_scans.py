"""Fetch selected CC0 Poly Haven sources, verifying each published checksum."""
from pathlib import Path
import urllib.request,json,hashlib,concurrent.futures
ROOT=Path(__file__).resolve().parent;OUT=ROOT/'scans';OUT.mkdir(exist_ok=True)
def get(url):
 with urllib.request.urlopen(urllib.request.Request(url,headers={'User-Agent':'VectorRushAssetAuthoring/1.0'}),timeout=90) as r:return r.read()
jobs=[];records=[]
for asset,key in [('concrete_panels','cast_concrete'),('corrugated_iron','corrugated_steel'),('asphalt_02','asphalt')]:
 d=json.loads(get('https://api.polyhaven.com/files/'+asset));folder=OUT/asset;folder.mkdir(exist_ok=True)
 (folder/'download-manifest.json').write_text(json.dumps(d,indent=2))
 for channel in ['Diffuse','nor_gl','Rough','AO','Displacement']:
  if channel not in d:continue
  entry=d[channel]['2k']['jpg'];name=channel+'.jpg';jobs.append((folder/name,entry))
  records.append(dict(asset=asset,material=key,channel=channel,file=f'{asset}/{name}',source=entry['url'],md5=entry['md5'],license='CC0-1.0',page='https://polyhaven.com/a/'+asset))
def fetch(job):
 path,e=job
 if path.exists() and hashlib.md5(path.read_bytes()).hexdigest()==e['md5']:return
 data=get(e['url']);assert hashlib.md5(data).hexdigest()==e['md5'];path.write_bytes(data);print(path.name,len(data),flush=True)
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(fetch,jobs))
(OUT/'provenance.json').write_text(json.dumps(records,indent=2))
# License declaration from the provider, stored alongside its source URL.
try:(OUT/'license-page.html').write_bytes(get('https://polyhaven.com/license'))
except Exception:pass
print('Verified',len(records),'CC0 source maps')
