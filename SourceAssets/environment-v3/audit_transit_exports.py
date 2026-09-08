"""Read-only export round trip. Run Blender factory startup, no file mutation."""
import bpy,bmesh,os,json,hashlib
from mathutils import Vector
ROOT=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(ROOT,'pass-02-transit')
EVIDENCE=os.path.abspath(os.path.join(ROOT,'../../evidence/environment-v3/pass-02-transit'))
with open(os.path.join(OUT,'asset-stats.json')) as f:source=json.load(f)
result={}
for name,expected in source.items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 path=os.path.join(OUT,name+'.fbx');bpy.ops.import_scene.fbx(filepath=path)
 obs=[o for o in bpy.context.scene.objects if o.type=='MESH'];vertices=[];triangles=nonmanifold=degenerate=0;uv_present=True;finite_tangents=True
 for ob in obs:
  vertices.extend([ob.matrix_world@v.co for v in ob.data.vertices]);triangles+=sum(len(p.vertices)-2 for p in ob.data.polygons)
  bm=bmesh.new();bm.from_mesh(ob.data);nonmanifold+=sum(not e.is_manifold for e in bm.edges);bm.free()
  degenerate+=sum(p.area<1e-10 for p in ob.data.polygons);uv_present=uv_present and bool(ob.data.uv_layers)
  ob.data.calc_tangents()
  import math
  finite_tangents=finite_tangents and all(all(math.isfinite(x) for x in l.tangent) and l.tangent.length>.99 for l in ob.data.loops)
 lo=[min(v[k] for v in vertices) for k in range(3)];hi=[max(v[k] for v in vertices) for k in range(3)]
 expected_lo=expected['source_min_xyz'];expected_hi=expected['source_max_xyz']
 bounds_error=max(abs(x-y) for x,y in zip(lo+hi,expected_lo+expected_hi))
 checks={'single_mesh':len(obs)==1,'triangle_count_matches':triangles==expected['triangles'],'bounds_match_within_0_1mm':bounds_error<.0001,'manifold':nonmanifold==0,'no_degenerate_triangles':degenerate==0,'uv0_present':uv_present,'valid_tangents':finite_tangents}
 result[name]={'checks':checks,'pass':all(checks.values()),'triangles':triangles,'nonmanifold_edges':nonmanifold,'degenerate_triangles':degenerate,'roundtrip_blender_bounds_min':lo,'roundtrip_blender_bounds_max':hi,'max_bounds_error_m':bounds_error,'fbx_sha256':hashlib.sha256(open(path,'rb').read()).hexdigest()}
with open(os.path.join(EVIDENCE,'export-roundtrip-audit.json'),'w') as f:json.dump(result,f,indent=2)
print(json.dumps(result,indent=2))
assert all(r['pass'] for r in result.values())
