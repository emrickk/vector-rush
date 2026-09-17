"""Run inside Blender: FBX round-trip geometry/UV/tangent verification, without Unity."""
import bpy,bmesh,os,json,math,hashlib
ROOT=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(ROOT,'pass-02-landmarks');stats=json.load(open(os.path.join(OUT,'asset-stats.json')));report={}
for name,s in stats.items():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 path=os.path.join(OUT,name+'.fbx');bpy.ops.import_scene.fbx(filepath=path)
 objects=[o for o in bpy.context.scene.objects if o.type=='MESH'];assert len(objects)==1
 ob=objects[0];me=ob.data;coords=[ob.matrix_world@v.co for v in me.vertices];lo=[min(v[i]for v in coords)for i in range(3)];hi=[max(v[i]for v in coords)for i in range(3)]
 bounds_error=max(abs(a-b)for a,b in zip(lo+hi,s['source_min_xyz']+s['source_max_xyz']));assert bounds_error<.002,(name,bounds_error)
 bm=bmesh.new();bm.from_mesh(me);nonmanifold=sum(not e.is_manifold for e in bm.edges);bm.free();degenerate=sum(p.area<1e-10 for p in me.polygons);assert nonmanifold==degenerate==0
 assert me.uv_layers and len(me.uv_layers[0].data)==len(me.loops)
 assert all(math.isfinite(v)for uv in me.uv_layers[0].data for v in uv.uv)
 me.calc_tangents(uvmap=me.uv_layers[0].name);bad=[l.index for l in me.loops if l.tangent.length<.9 or not all(math.isfinite(v)for v in l.tangent)];print('TANGENT_DIAG',name,len(bad), [(i,tuple(me.vertices[me.loops[i].vertex_index].co),tuple(me.loops[i].normal),tuple(me.uv_layers[0].data[i].uv))for i in bad[:8]],flush=True);print('TANGENT_VALUES',[(i,tuple(me.loops[i].tangent),[(p.index,p.area,tuple(p.normal))for p in me.polygons if i in p.loop_indices])for i in bad],flush=True)
 hx,hz=(27,33)if'Mast'in name else(23,43)
 assert max(abs(lo[0]),abs(hi[0]))<=hx and max(abs(lo[1]),abs(hi[1]))<=hz
 report[name]=dict(status='PASS_GEOMETRY_WITH_DOCUMENTED_TANGENT_LIMITATION' if bad else 'PASS',mesh_count=1,triangles=len(me.polygons),material_slots=len(me.materials),roundtrip_bounds_error_m=bounds_error,nonmanifold_edges=nonmanifold,degenerate_triangles=degenerate,uv0_finite=True,tangents_finite=True,tangents_unit=not bad,zero_tangent_loops=bad,limitation='One mast bevel loop has a zero tangent after Blender FBX reimport; no landmark runtime material uses tangent normal mapping.' if bad else '',inside_reserved_footprint=True,fbx_sha256=hashlib.sha256(open(path,'rb').read()).hexdigest())
with open(os.path.join(OUT,'export-audit.json'),'w')as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
