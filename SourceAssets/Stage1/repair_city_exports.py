"""Correct Blender-to-Unity handedness at source; keep authored Unity bounds unchanged.
Run with Blender through the machine-wide heavy-process lease.
"""
import bpy,bmesh,json,hashlib,shutil
from pathlib import Path
from mathutils import Matrix
root=Path(__file__).resolve().parents[2]
source=root/'SourceAssets/Experience/packages/vrx-b01'
out=root/'SourceAssets/Stage1/city-s1'
if out.exists(): raise RuntimeError('Fresh output required')
shutil.copytree(source,out)
bpy.ops.wm.open_mainfile(filepath=str(source/'source/vrx_city_kit.blend'))
bpy.context.preferences.filepaths.save_version=0
manifest=json.loads((out/'manifest.json').read_text())
for a in manifest['assets']:
 for lod in (0,1):
  name=a['id']+'_LOD'+str(lod)
  ob=bpy.data.objects[name]
  for c in ob.users_collection: c.hide_viewport=False;c.hide_render=False
  ob.hide_set(False)
  bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
  # Original source stored Unity +Z as Blender -Y; FBX + Unity axis baking
  # already supplies the handedness change. Reflect once here and repair normals.
  ob.data.transform(Matrix.Diagonal((1,-1,1,1)))
  bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(ob.data);bm.free()
  bpy.ops.export_scene.fbx(filepath=str(out/a['lod'+str(lod)]),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False)
  print('SOURCE_REPAIRED',name,flush=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'source/vrx_city_kit.blend'))
for name in ('manifest.json','layout.json','lighting.json','experience-assets.json'):
 p=out/name;d=json.loads(p.read_text());d['revision']='city-s1';p.write_text(json.dumps(d,indent=2)+'\n')
shutil.copy2(__file__,out/'repair_city_exports.py')
checks={'contractVersion':1,'revision':'city-s1','files':[{'path':str(p.relative_to(out)),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in sorted(out.rglob('*')) if p.is_file() and p.name not in ('checksums.json','READY.json')]}
(out/'checksums.json').write_text(json.dumps(checks,indent=2)+'\n')
ready={'contractVersion':1,'revision':'city-s1','courseHash':manifest['courseHash'],'checksumsSha256':hashlib.sha256((out/'checksums.json').read_bytes()).hexdigest()}
(out/'READY.json').write_text(json.dumps(ready,indent=2)+'\n')
