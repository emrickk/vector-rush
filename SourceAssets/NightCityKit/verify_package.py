"""Verify delivered source/engine parity and references without regenerating assets."""
from pathlib import Path
import hashlib, json, re, subprocess
from PIL import Image

root=Path(__file__).resolve().parent; repo=root.parents[1]
engine=repo/'UnityProject/Assets/Art/NightCityKit'
out=repo/'docs/night-city-art-2026-09-17'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
materials=json.loads((root/'materials.json').read_text())['materials']
meshes=json.loads((root/'mesh-manifest.json').read_text())['assets']
checks=[]
for record in meshes:
    source=root/'meshes'/record['file']; imported=engine/'Models'/record['file']
    assert sha(source)==sha(imported),source
    assert record['triangles']>0 and record['uv'],source
for record in materials:
    assert (engine/'Materials'/(record['id']+'.mat')).is_file(),record['id']
    for slot in ['base','normal','ao','ms','emission']:
        if not record[slot]:continue
        source=root/'textures'/record[slot]; imported=engine/'Textures'/record[slot]
        assert sha(source)==sha(imported),source
        with Image.open(source) as image: image.verify()
        meta=Path(str(imported)+'.meta').read_text()
        expected='0' if slot in ['normal','ao','ms'] else '1'
        assert re.search(r'sRGBTexture:\s*'+expected+r'\b',meta),imported
        if slot=='normal':assert re.search(r'textureType:\s*1\b',meta),imported
checks.append('38 FBX exports and every bound source texture byte-match engine assets')
checks.append('Every bound PNG decodes; normal and data maps have linear import settings')
ids=sorted({x['id'] for x in meshes})
assert len(ids)==19 and len(meshes)==38
for asset in ids:
    assert sorted(x['lod'] for x in meshes if x['id']==asset)==[0,1]
    assert (engine/'Prefabs'/('NC_'+asset+'.prefab')).exists()
checks.append('19 prefabs each have LOD0/LOD1 source and exports')
profile=(engine/'ReviewVolume.asset').read_text()
assert 'm_Name: Tonemapping' in profile and 'm_Name: Bloom' in profile
component_list=profile.split('  components:',1)[1].split('\n---',1)[0]
assert '{fileID: 0}' not in component_list
checks.append('Review tonemapping and bloom serialize as non-null subassets')
scene_paths=subprocess.check_output(['git','ls-files','UnityProject/Assets/Scenes'],cwd=repo,text=True).splitlines()
for name in scene_paths:
    expected=subprocess.check_output(['git','show','HEAD:'+name],cwd=repo)
    assert (repo/name).read_bytes()==expected,name
checks.append('All tracked racing scenes and their metadata remain byte-identical to branch baseline')
for record in json.loads((root/'scans/provenance.json').read_text()):
    assert hashlib.md5((root/'scans'/record['file']).read_bytes()).hexdigest()==record['md5'],record['file']
checks.append('15 scan-source maps match recorded provider checksums')
for path in engine.rglob('*'):
    if path.name.endswith('.meta'):continue
    assert Path(str(path)+'.meta').exists(),path
checks.append('All engine kit files and directories retain Unity metadata')
files=[]
for folder in [root,engine,repo/'UnityProject/Assets/Editor/NightCityKit']:
    for path in sorted(folder.rglob('*')):
        if not path.is_file() or path.suffix in ['.blend1','.pyc'] or '__pycache__' in path.parts:continue
        files.append(dict(path=str(path.relative_to(repo)),bytes=path.stat().st_size,sha256=sha(path)))
(out/'inventory.json').write_text(json.dumps(dict(scope='NightCityKit source and engine assets',files=files),indent=2)+'\n')
(out/'package-validation.json').write_text(json.dumps(dict(status='PASS',prefabs=19,meshes=38,materialDefinitions=len(materials),unityMaterials=len(list((engine/'Materials').glob('*.mat'))),checks=checks),indent=2)+'\n')
print('PACKAGE_VERIFIED',len(files),'files',len(checks),'checks')
