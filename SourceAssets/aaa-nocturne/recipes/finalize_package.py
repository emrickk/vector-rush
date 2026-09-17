"""Package integrity only; no renderer, Unity, network, or git access."""
from pathlib import Path
import json,hashlib,struct,zlib,shutil
root=Path(__file__).resolve().parent.parent
out=root/'exemplar-01'
manifest=json.loads((out/'manifest.json').read_text())
stats=json.loads((out/'asset-stats.json').read_text())
rt=json.loads((out/'checks/fbx-roundtrip.json').read_text())
clearance=json.loads((out/'checks/placement-clearance.json').read_text())
assert 'COMPLETE_NONRENDERING_AUTHORING' in (out/'checks/blender-build.log').read_text()
for s in stats.values():
 for k in ['nonmanifoldEdges','degenerateTriangles','duplicateTriangles','zeroAreaUVTriangles','nonpositiveComponents']:assert s[k]==0
for s in clearance.values():assert s['surfaceTriangleIntersections']==s['verticesInsideProtectedCorridorSamples']==0
for asset in manifest['assets']:
 for lod in ['lod0','lod1']:
  p=out/asset[lod];assert hashlib.sha256(p.read_bytes()).hexdigest()==stats[p.stem]['fbxSha256']
 refs=asset['materialSlots'];assert all(any(m['id']==r for m in manifest['materials']) for r in refs)
texture_checks={}
for p in (out/'textures').glob('*.png'):
 b=p.read_bytes();assert b[:8]==b'\x89PNG\r\n\x1a\n';pos=8;raw=b''
 while pos<len(b):
  n=struct.unpack('>I',b[pos:pos+4])[0];t=b[pos+4:pos+8];v=b[pos+8:pos+8+n];crc=struct.unpack('>I',b[pos+8+n:pos+12+n])[0];assert zlib.crc32(t+v)&0xffffffff==crc
  if t==b'IDAT':raw+=v
  if t==b'IHDR':w,h,depth,mode,_,_,_=struct.unpack('>IIBBBBB',v)
  pos+=n+12
 decoded=zlib.decompress(raw);assert len(decoded)==h*(1+w*(4 if mode==6 else 3));texture_checks[p.name]={'size':[w,h],'bitDepth':depth,'channels':4 if mode==6 else 3,'crcAndDecompression':'pass'}
(out/'checks/texture-integrity.json').write_text(json.dumps(texture_checks,indent=2))
shutil.copy2(root/'recipes/build_exemplar.py',out/'source/recipe_snapshot.py')
# Blender's automatic local backup is not part of the export payload.
for p in (out/'source').glob('*.blend1'):p.unlink()
checksums={str(p.relative_to(out)):hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(out.rglob('*')) if p.is_file() and p.name not in ['checksums.json','STAGED.json']}
(out/'checksums.json').write_text(json.dumps(checksums,indent=2))
(out/'STAGED.json').write_text(json.dumps({'revision':'exemplar-01','status':'TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW','courseHash':manifest['courseHash'],'checksumsSha256':hashlib.sha256((out/'checksums.json').read_bytes()).hexdigest(),'layoutComplete':False,'nativeImportVerified':False,'rendered':False},indent=2))
print('PACKAGE_INTEGRITY_PASS',len(stats),'FBX',len(texture_checks),'maps',len(clearance),'proposed instances')
for a in manifest['assets']:
 s=stats[a['id']+'_LOD0'];l=stats[a['id']+'_LOD1'];print(a['id'],s['triangles'],l['triangles'],[round(b-a,3) for a,b in zip(s['unityBoundsMin'],s['unityBoundsMax'])])
print('MAX_ROUNDTRIP_BOUNDS_ERROR',max(x['boundsMaxErrorMeters'] for x in rt.values()))
print('COURSE_HASH',manifest['courseHash'])
