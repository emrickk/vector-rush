"""Bounded night variants: keep authored geometry; assign a few occupied floors."""
import bpy,os,json,hashlib,math,shutil
from mathutils import Vector
ROOT=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(ROOT,'pass-03-city-correction')
EVIDENCE=os.path.abspath(os.path.join(ROOT,'../../evidence/environment-v3/pass-03-city-correction'))
LIVE=os.path.abspath(os.path.join(ROOT,'../../UnityProject/Assets/Resources/Art/Environment'))
DEST=os.path.join(OUT,'Nocturne_NightTower_Kit.blend')
if os.path.exists(DEST):raise RuntimeError('Immutable source pass already exists')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=16;scene.cycles.use_denoising=False
scene.render.threads_mode='FIXED';scene.render.threads=4;scene.render.resolution_x=1200;scene.render.resolution_y=1050;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
warm=bpy.data.materials.new('WarmWindow');warm.diffuse_color=(.55,.31,.14,1);warm.use_nodes=True
p=warm.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(.55,.31,.14,1);p.inputs['Emission Color'].default_value=(.8,.38,.13,1);p.inputs['Emission Strength'].default_value=.8
assets=[];stats={}
for original,new,family in [('Solstice_TerraceTower_A','Nocturne_TerraceTower_Night_A','terrace'),('Solstice_SplitTower_B','Nocturne_SplitTower_Night_B','split')]:
 source=os.path.join(LIVE,original+'.fbx');bpy.ops.import_scene.fbx(filepath=source)
 ob=next(o for o in bpy.context.selected_objects if o.type=='MESH');ob.name=new
 bpy.context.view_layer.objects.active=ob
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 ob.data.update()
 original_coords=[tuple(v.co) for v in ob.data.vertices];original_polys=[tuple(p.vertices) for p in ob.data.polygons]
 warm_slot=len(ob.data.materials);ob.data.materials.append(warm);assigned=0
 for poly in ob.data.polygons:
  material=ob.data.materials[poly.material_index]
  if not material or 'Glass' not in material.name:continue
  c=poly.center;n=poly.normal
  if family=='terrace':
   floor=int(math.floor((c.z-7.3)/3.35));lit=floor in [2,3,9,10,15] and ((n.y<-.9 and c.y<-5) or (n.x<-.9 and c.x<-5 and floor in [3,10]))
  else:
   floor=int(math.floor((c.z-6.91)/3.5));lit=floor in [3,4,10,11] and abs(n.x)>.9 and abs(c.x)>10
  if lit:poly.material_index=warm_slot;assigned+=1
 assert assigned>0
 assert original_coords==[tuple(v.co) for v in ob.data.vertices]
 assert original_polys==[tuple(p.vertices) for p in ob.data.polygons]
 # Retain original UVs/topology. This is a material-assignment variant only.
 for m in ob.data.materials:
  if not m or m==warm:continue
  m.use_nodes=True;node=m.node_tree.nodes.get('Principled BSDF')
  if not node:continue
  col=(.027,.068,.089,1) if 'Glass' in m.name else (.38,.46,.51,1) if 'Ivory' in m.name else (.055,.075,.091,1)
  node.inputs['Base Color'].default_value=col;m.diffuse_color=col
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 path=os.path.join(OUT,new+'.fbx')
 bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False)
 coords=[v.co for v in ob.data.vertices];lo=[min(v[k] for v in coords) for k in range(3)];hi=[max(v[k] for v in coords) for k in range(3)]
 stats[new]={'source_fbx':source,'source_fbx_sha256':hashlib.sha256(open(source,'rb').read()).hexdigest(),'variant_fbx_sha256':hashlib.sha256(open(path,'rb').read()).hexdigest(),'vertices':len(ob.data.vertices),'triangles':sum(len(p.vertices)-2 for p in ob.data.polygons),'polygons_given_occupied_material':assigned,'material_assignment_only':True,'source_bounds_min_xyz':lo,'source_bounds_max_xyz':hi,'unity_dimensions_xyz':[hi[0]-lo[0],hi[2]-lo[2],hi[1]-lo[1]],'source_zero_area_triangles_retained':sum(p.area<1e-10 for p in ob.data.polygons),'uv_note':'Original authored UVs retained. No texture bake or UV readiness claim.'}
 assets.append(ob)
assets[0].location.x=-20;assets[1].location.x=20
world=bpy.data.worlds.new('Night variant inspection');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.08,.12,.17,1);world.node_tree.nodes['Background'].inputs[1].default_value=.35
bpy.ops.mesh.primitive_plane_add(size=1000,location=(0,0,-.05));ground=bpy.context.object;ground.name='STUDIO ground'
mat=bpy.data.materials.new('STUDIO ground material');mat.diffuse_color=(.05,.065,.08,1);ground.data.materials.append(mat)
for name,loc,energy,size in [('Cool key',(-60,-45,95),75000,55),('Shape fill',(45,10,65),40000,40)]:
 data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.size=size;light=bpy.data.objects.new(name,data);scene.collection.objects.link(light);light.location=loc;light.rotation_euler=(Vector((0,0,32))-light.location).to_track_quat('-Z','Y').to_euler()
data=bpy.data.cameras.new('Inspection');cam=bpy.data.objects.new('Inspection',data);scene.collection.objects.link(cam);cam.location=(-95,-140,77);cam.rotation_euler=(Vector((0,0,36))-cam.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=99;scene.camera=cam
scene['Scope']='Material-assignment-only derivatives of existing authored towers. No vertex/polygon changes. Native facing and lighting still require inspection.'
with open(os.path.join(OUT,'night-tower-stats.json'),'w') as f:json.dump(stats,f,indent=2)
shutil.copyfile(__file__,os.path.join(OUT,'night-tower-recipe-snapshot.py'))
bpy.ops.wm.save_as_mainfile(filepath=DEST)
print(json.dumps(stats,indent=2),flush=True)
scene.render.filepath=os.path.join(EVIDENCE,'01-night-tower-groups-studio.png');bpy.ops.render.render(write_still=True)
