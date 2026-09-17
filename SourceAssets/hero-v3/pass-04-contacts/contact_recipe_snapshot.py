"""Immutable pass04 contact correction. Input saved pass03-finish; no UV re-atlas or texture bake."""
import bpy,json,sys,runpy,shutil,hashlib
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];OUT=Path(args[0]);EVIDENCE=Path(args[1]);SOURCE=Path(bpy.data.filepath);root=Path(__file__).parent
OUT.mkdir(parents=True,exist_ok=True);EVIDENCE.mkdir(parents=True,exist_ok=True)
if (OUT/'Kestrel07-v3.blend').exists():raise RuntimeError('Preserve existing revision; choose a new directory.')
def fingerprints(collection,mode):
 result={}
 for obj in collection.objects:
  if obj.type!='MESH':continue
  values=([list(v.uv) for v in obj.data.uv_layers.active.data] if mode=='UV' else {'v':[list(v.co) for v in obj.data.vertices],'f':[list(p.vertices) for p in obj.data.polygons]})
  result[obj.name]=hashlib.sha256(json.dumps(values,sort_keys=True).encode()).hexdigest()
 return result
runtime=bpy.data.collections['EXPORT_RUNTIME'];craft=bpy.data.collections['KESTREL_07_EXPORT'];before_uv=fingerprints(runtime,'UV');before_geometry=fingerprints(runtime,'geometry');before_source=fingerprints(craft,'geometry')
correction=runpy.run_path(str(root/'contact_correction_v3.py'))['apply_contact_correction']()
after_uv=fingerprints(runtime,'UV');after_geometry=fingerprints(runtime,'geometry');after_source=fingerprints(craft,'geometry')
changed_runtime=[k for k in before_geometry if before_geometry[k]!=after_geometry[k]];changed_source=[k for k in before_source if before_source[k]!=after_source[k]]
assert before_uv==after_uv,'UV coordinates changed unexpectedly'
assert changed_runtime==['Kestrel07_V3_Graphite'],'Unintended runtime mesh changed: '+str(changed_runtime)
assert set(changed_source)==set(correction['source_moved_vertices']),'Unintended source mesh changed'
for f in ['engine-anchors.json','finish-contract.json','ship-livery-mask.png','livery-mask-contract.json','form_recipe_snapshot.py','finish_recipe_snapshot.py']:shutil.copy2(SOURCE.parent/f,OUT/f)
shutil.copy2(__file__,OUT/'contact_recipe_snapshot.py');shutil.copy2(root/'contact_correction_v3.py',OUT/'contact_correction_v3.py')
correction.update({'input_source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'all_runtime_uvs_identical':before_uv==after_uv,'runtime_geometry_changed':changed_runtime,'source_geometry_changed':changed_source,'before_runtime_uv_sha256':before_uv,'after_runtime_uv_sha256':after_uv,'textures_baked':False,'native_validation_pending':True})
(OUT/'contact-correction-contract.json').write_text(json.dumps(correction,indent=2))
runtime.hide_viewport=False;runtime.hide_render=False;bpy.ops.object.select_all(action='DESELECT')
for obj in runtime.objects:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'HeroShip.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,use_tspace=True,path_mode='AUTO')
runtime.hide_render=True;runtime.hide_viewport=True
stats=json.loads((SOURCE.parent/'asset-stats.json').read_text());stats.update({'revision':OUT.name,'contact_source_sha256':correction['input_source_sha256'],'contact_recipe_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'contact_scope':'Only graphite sill and nacelle backing surfaces moved to resolve measured depth conflicts. Armor, glazing, engines and all runtime UVs unchanged.','runtime_uv_policy':'Every pass03 runtime UV coordinate is preserved verbatim. Only Graphite vertex positions changed; exact final FBX is EXPORT_RUNTIME. No texture bake performed.'})
(OUT/'asset-stats.json').write_text(json.dumps(stats,indent=2))
scene=bpy.context.scene;scene.render.threads_mode='FIXED';scene.render.threads=4;scene.cycles.samples=32;scene.cycles.use_denoising=False
(EVIDENCE/'capture-contract.json').write_text(json.dumps({'revision':OUT.name,'resolution':[1600,1200],'samples':32,'denoising':False,'threads':4,'bloom':False,'neutral_white_studio':True,'views':[('01-neutral-rear-quarter',(-8,10,6),(0,0,.2),9.6,False,'ORTHO')],'completion_status':'Geometry/audit revision; native before/after validation pending; no source render requested yet'},indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Kestrel07-v3.blend'))
print('CONTACT_CORRECTED_SOURCE_READY',json.dumps(correction),flush=True)
sys.argv=['diagnostic','--',str(EVIDENCE/'surface-contacts-after.json')];runpy.run_path(str(root/'diagnose_surface_contacts.py'),run_name='__main__')
