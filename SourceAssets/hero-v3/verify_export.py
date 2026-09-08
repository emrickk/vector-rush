"""Audit immutable source + exact runtime FBX; write evidence only, never save source."""
import bpy,bmesh,json,sys,hashlib,math
from pathlib import Path
from mathutils import Vector
argv=sys.argv[sys.argv.index('--')+1:]
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
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(fbx))
report['reimported_fbx_meshes']=inspect(bpy.context.scene.objects)
report['all_source_manifold']=all(o['nonmanifold_edges']==0 for o in report['source_meshes'].values())
report['all_runtime_manifold_and_triangulated']=all(o['nonmanifold_edges']==0 and o['polygons']==o['triangles'] for o in report['runtime_meshes'].values())
report['all_fbx_manifold_with_valid_tangents']=all(o['nonmanifold_edges']==0 and o.get('valid_tangents',False) for o in report['reimported_fbx_meshes'].values())
report['runtime_fbx_triangle_count_matches']=sum(o['triangles'] for o in report['runtime_meshes'].values())==sum(o['triangles'] for o in report['reimported_fbx_meshes'].values())
destination.write_text(json.dumps(report,indent=2))
print(json.dumps({k:v for k,v in report.items() if not isinstance(v,dict)},indent=2))
