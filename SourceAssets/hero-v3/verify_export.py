"""Audit immutable source + exact runtime FBX; write evidence only, never save source."""
import bpy,bmesh,json,sys,hashlib,math
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:]
def stable_mesh_fingerprints(collection,uv=False):
 result={}
 for obj in collection.objects:
  if obj.type!='MESH':continue
  values=([list(loop.uv) for loop in obj.data.uv_layers.active.data] if uv else {'vertices':[list(v.co) for v in obj.data.vertices],'faces':[list(p.vertices) for p in obj.data.polygons],'matrix':[list(r) for r in obj.matrix_world]})
  result[obj.name]=hashlib.sha256(json.dumps(values,sort_keys=True).encode()).hexdigest()
 return result
source=Path(bpy.data.filepath);fbx=Path(argv[0]);destination=Path(argv[1])
def inspect(objects):
 result={}
 for o in objects:
  if o.type!='MESH':continue
  bm=bmesh.new();bm.from_mesh(o.data)
  item={'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'triangles':sum(len(p.vertices)-2 for p in o.data.polygons),'ngons_over_quad':sum(len(p.vertices)>4 for p in o.data.polygons),'nonmanifold_edges':sum(not e.is_manifold for e in bm.edges),'uv_layers':[u.name for u in o.data.uv_layers]}
  bm.free()
  if o.data.uv_layers:
   uv=o.data.uv_layers.active.data
   item['uv_bounds']=[min(v.uv.x for v in uv),min(v.uv.y for v in uv),max(v.uv.x for v in uv),max(v.uv.y for v in uv)]
   try:
    o.data.calc_tangents();item['valid_tangents']=all(all(math.isfinite(v) for v in loop.tangent) for loop in o.data.loops)
   except Exception as e:item['valid_tangents']=False;item['tangent_error']=str(e)
  result[o.name]=item
 return result
report={'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'fbx_sha256':hashlib.sha256(fbx.read_bytes()).hexdigest(),'source_meshes':inspect(bpy.data.collections['KESTREL_07_EXPORT'].objects),'runtime_meshes':inspect(bpy.data.collections['EXPORT_RUNTIME'].objects)}
source_shapes=stable_mesh_fingerprints(bpy.data.collections['KESTREL_07_EXPORT'])
source_uvs=stable_mesh_fingerprints(bpy.data.collections['EXPORT_RUNTIME'],True)
if 'SHIP_LIVERY' in bpy.data.node_groups:
 report['livery']={'group': 'SHIP_LIVERY','materials':[m.name for m in bpy.data.materials if m.use_nodes and m.node_tree.nodes.get('SHIP_LIVERY')],'images':[{'name':i.name,'packed':bool(i.packed_file),'size':list(i.size),'colorspace':i.colorspace_settings.name} for i in bpy.data.images if 'livery' in i.name]}
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(fbx))
report['reimported_fbx_meshes']=inspect(bpy.context.scene.objects)
report['all_source_manifold']=all(o['nonmanifold_edges']==0 for o in report['source_meshes'].values())
report['all_runtime_manifold_and_triangulated']=all(o['nonmanifold_edges']==0 and o['polygons']==o['triangles'] for o in report['runtime_meshes'].values())
report['all_fbx_manifold_with_valid_tangents']=all(o['nonmanifold_edges']==0 and o.get('valid_tangents',False) for o in report['reimported_fbx_meshes'].values())
report['runtime_fbx_triangle_count_matches']=sum(o['triangles'] for o in report['runtime_meshes'].values())==sum(o['triangles'] for o in report['reimported_fbx_meshes'].values())
if len(argv)>2:
 bpy.ops.wm.open_mainfile(filepath=argv[2])
 report['major_source_geometry_matches_baseline']=source_shapes==stable_mesh_fingerprints(bpy.data.collections['KESTREL_07_EXPORT'])
 baseline_uvs=stable_mesh_fingerprints(bpy.data.collections['EXPORT_RUNTIME'],True)
 report['coated_glass_runtime_uvs_unchanged']=all(source_uvs.get(k)==v for k,v in baseline_uvs.items() if not k.endswith('_Engine'))
destination.write_text(json.dumps(report,indent=2))
print(json.dumps({k:v for k,v in report.items() if not isinstance(v,dict)},indent=2))
