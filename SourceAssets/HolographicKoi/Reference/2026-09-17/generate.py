"""Generate the owner-requested polish reference via the existing Scenario API."""
import json,os,sys,urllib.request
from pathlib import Path
root=Path(__file__).resolve().parent
model='model_openai-gpt-image-2'
def api(path,body=None):
 request=urllib.request.Request('https://api.cloud.scenario.com/v1'+path,data=None if body is None else json.dumps(body).encode(),headers={'Authorization':'Basic '+os.environ['SCENARIO_API_TOKEN'],'Content-Type':'application/json'})
 with urllib.request.urlopen(request,timeout=120) as response:return json.load(response)
if sys.argv[1]=='submit':
 if (root/'job.json').exists():raise SystemExit('Existing job preserved; use poll.')
 body=json.loads((root/'request.json').read_text());job=api('/generate/custom/'+model,body)['job']['jobId']
 (root/'job.json').write_text(json.dumps(dict(provider='Scenario',model=model,jobId=job),indent=2)+'\n');print('Submitted',job)
else:
 job=api('/jobs/'+json.loads((root/'job.json').read_text())['jobId'])['job'];print('Status:',job['status'])
 if job['status']=='success':
  aid=job['metadata']['assetIds'][0];asset=api('/assets/'+aid)['asset']
  with urllib.request.urlopen(asset['url'],timeout=120) as response:(root/'koi-overhead-target-01.png').write_bytes(response.read())
  (root/'asset.json').write_text(json.dumps(dict(assetId=aid),indent=2)+'\n');print(root/'koi-overhead-target-01.png')
