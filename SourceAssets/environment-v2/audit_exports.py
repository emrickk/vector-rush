import bpy,os,json,math,hashlib
from mathutils import Vector
from io_scene_fbx import parse_fbx
ROOT=os.path.dirname(os.path.abspath(__file__))
report={}
for filename in ['Solstice_TerraceTower_A.fbx','Solstice_SplitTower_B.fbx','Solstice_CoastalCliff_C.fbx']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 for mat in list(bpy.data.materials):
  if mat.users==0:bpy.data.materials.remove(mat)
 p=os.path.join(ROOT,'exports',filename)
 tree,version=parse_fbx.parse(p)
 globalset=next(e for e in tree.elems if e.id==b'GlobalSettings')
 props=next(e for e in globalset.elems if e.id==b'Properties70')
 axes={str(e.props[0],'utf8'):e.props[-1] for e in props.elems if e.id==b'P' and e.props[0] in [b'UpAxis',b'UpAxisSign',b'FrontAxis',b'FrontAxisSign',b'UnitScaleFactor']}
 bpy.ops.import_scene.fbx(filepath=p,use_custom_normals=True)
 objs=[o for o in bpy.context.scene.objects if o.type=='MESH'];meshes=[]
 for ob in objs:
  me=ob.data;me.calc_loop_triangles();coords=[ob.matrix_world@Vector(c) for c in ob.bound_box]
  meshes.append({'name':ob.name,'dimensions_blender_xyz_m':[max(c[j] for c in coords)-min(c[j] for c in coords) for j in range(3)],'triangles':len(me.loop_triangles),'degenerate_triangles':sum(t.area<1e-10 for t in me.loop_triangles),'uv_layers':len(me.uv_layers),'uv_layer_names':[uv.name for uv in me.uv_layers],'uv0_bounds':[[min(v.uv[j] for v in me.uv_layers[0].data),max(v.uv[j] for v in me.uv_layers[0].data)] for j in range(2)] if me.uv_layers else None,'uv0_nonzero_loop_fraction':sum(v.uv.length>1e-8 for v in me.uv_layers[0].data)/len(me.uv_layers[0].data) if me.uv_layers else 0,'materials':[m.name for m in me.materials],'vertices_all_finite':all(math.isfinite(v) for p in me.vertices for v in p.co),'origin_world':list(ob.matrix_world.translation)})
 report[filename]={'sha256':hashlib.sha256(open(p,'rb').read()).hexdigest(),'fbx_version':version,'fbx_global_settings':axes,'meshes':meshes}
json.dump(report,open(os.path.join(ROOT,'export_audit.json'),'w'),indent=2)
print(json.dumps(report,indent=2),flush=True)
